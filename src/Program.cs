using System.Text;
using DotNetEnv;
using FluentValidation;
using KidFit.Data;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Services;
using KidFit.Shared.Constants;
using KidFit.Validators;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;

// Load .env
// This is just for local development, since we can use Docker to inject environment variables
// so no need to check if this failed or not
Env.Load();

// Create builder
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

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    // options.AccessDeniedPath = "/Auth/AccessDenied";
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
    .AddPolicy("AdminOrStaff", policy => policy.RequireRole(Role.ADMIN.ToString(), Role.STAFF.ToString()));

// Add custom claims factory
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsPrincipalFactory>();

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
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<MailService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Register validators
builder.Services.AddScoped<IValidator<CardCategory>, CardCategoryValidator>();
builder.Services.AddScoped<IValidator<Card>, CardValidator>();
builder.Services.AddScoped<IValidator<Module>, ModuleValidator>();
builder.Services.AddScoped<IValidator<Lesson>, LessonValidator>();
builder.Services.AddScoped<IValidator<ApplicationUser>, ApplicationUserValidator>();

// Add TickerQ
builder.Services.AddTickerQ(options =>
{
    // Core configuration
    options.ConfigureScheduler(schedulerOptions =>
    {
        schedulerOptions.MaxConcurrency = 10;
        schedulerOptions.NodeIdentifier = "notification-server";
    });

    // Dashboard
    options.AddDashboard(dashboardOptions =>
    {
        dashboardOptions.SetBasePath("/admin/tickerq");
        dashboardOptions.WithBasicAuth("admin", builder.Configuration["AppSettings:DefaultAdminPassword"]);
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
    // app.UseSwagger();
    // app.UseSwaggerUI();
    // app.UseHttpLogging();

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

    // Use developer exception handler
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
}
else
{
    app.UseExceptionHandler("/Error/500");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseHsts();
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
        var roleService = provider.GetRequiredService<RoleService>();
        var uow = provider.GetRequiredService<IUnitOfWork>();
        var logger = provider.GetRequiredService<ILogger<DbInitilizer>>();
        var config = provider.GetRequiredService<IConfiguration>();

        await DbInitilizer.InitializeAsync(context, accountService, roleService, uow, config, logger);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to initialize database: {ex.Message}");
        return;
    }
}

app.Run();
