using System.Security.Claims;
using System.Text;
using DotNetEnv;
using FluentValidation;
using KidFit.Data;
using KidFit.Dtos;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Services;
using KidFit.Shared.Constants;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;

// Load .env
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Register controllers with views
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConn"));
});

// Implement Options pattern
builder.Services.Configure<MailServiceOptions>(builder.Configuration.GetSection("AppSettings"));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

// Add JWT Authentication
var jwtSecret = builder.Configuration["AppSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
var jwtIssuer = builder.Configuration["AppSettings:Issuer"] ?? "KidFit";
var jwtAudience = builder.Configuration["AppSettings:Audience"] ?? "KidFit";

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// Setup authorization policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole(Role.ADMIN.ToString()))
    .AddPolicy("StaffOnly", policy => policy.RequireRole(Role.STAFF.ToString()))
    .AddPolicy("AdminOrStaff", policy => policy.RequireRole(Role.ADMIN.ToString(), Role.STAFF.ToString()))
    .AddPolicy("AdminOrSelf", policy => policy.RequireAssertion(context =>
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var routeId = context.Resource switch
        {
            HttpContext http => http.Request.RouteValues["id"]?.ToString(),
            _ => null
        };

        return userId == routeId || context.User.IsInRole(Role.ADMIN.ToString());
    }));

// Register AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Register Unit Of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Generic repository
builder.Services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));

// Register services
builder.Services.AddScoped<CardCategoryService>();
builder.Services.AddScoped<CardService>();
builder.Services.AddScoped<ModuleService>();
builder.Services.AddScoped<LessonService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<MailService>();
// builder.Services.AddScoped<ITimeTickerManager<TimeTickerEntity>, >();

// Register validators
builder.Services.AddScoped<IValidator<CardCategory>, CardCategoryValidator>();
builder.Services.AddScoped<IValidator<Card>, CardValidator>();
builder.Services.AddScoped<IValidator<Module>, ModuleValidator>();
builder.Services.AddScoped<IValidator<Lesson>, LessonValidator>();
builder.Services.AddScoped<IValidator<LoginRequestDto>, LoginRequestValidator>();

// Add TickerQ
builder.Services.AddTickerQ(options =>
{
    // Core configuration
    options.ConfigureScheduler(schedulerOptions =>
    {
        schedulerOptions.MaxConcurrency = 10;
        schedulerOptions.NodeIdentifier = "notification-server";
    });

    // options.SetExceptionHandler<NotificationExceptionHandler>();

    // Entity Framework persistence using built-in TickerQDbContext
    // Dashboard
    options.AddDashboard(dashboardOptions =>
    {
        dashboardOptions.SetBasePath("/admin/tickerq");
        dashboardOptions.WithBasicAuth("admin", "secure-password");
    });
});

// Register API logging
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath |
                            HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
});

// Register ProblemDetails 
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Define the BearerAuth scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {your token}"
    });

    // Require Bearer token globally (optional but recommended)
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpLogging();
    // app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}

app.UseTickerQ();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Run db initilizer
using (var scope = app.Services.CreateScope())
{
    try
    {
        var provider = scope.ServiceProvider;
        var context = provider.GetRequiredService<AppDbContext>();
        var accountService = provider.GetRequiredService<AccountService>();
        var uow = provider.GetRequiredService<IUnitOfWork>();
        var logger = provider.GetRequiredService<ILogger<DbInitilizer>>();
        var config = provider.GetRequiredService<IConfiguration>();

        await DbInitilizer.InitializeAsync(context, accountService, uow, config, logger);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to initialize database: {ex.Message}");
    }
}

app.Run();
