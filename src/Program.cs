using System.Text;
using FluentValidation;
using KidFit.Data;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Services;
using KidFit.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConn"));
});

// Add Identity
// builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
// {
//     options.Password.RequireDigit = true;
//     options.Password.RequireLowercase = true;
//     options.Password.RequireNonAlphanumeric = false;
//     options.Password.RequireUppercase = true;
//     options.Password.RequiredLength = 8;
//     options.User.RequireUniqueEmail = true;
// })
// .AddEntityFrameworkStores<AppDbContext>()
// .AddDefaultTokenProviders();

// Add JWT Authentication
// var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
// var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "KidFit";
// var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "KidFit";
//
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = jwtIssuer,
//         ValidAudience = jwtAudience,
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
//     };
// });
//
// builder.Services.AddAuthorizationBuilder()
//     .AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"))
//     .AddPolicy("StaffOnly", policy => policy.RequireRole("STAFF"))
//     .AddPolicy("AdminOrStaff", policy => policy.RequireRole("ADMIN", "STAFF"));

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
// builder.Services.AddScoped<AuthService>();

// Register validators
builder.Services.AddScoped<IValidator<CardCategory>, CardCategoryValidation>();
builder.Services.AddScoped<IValidator<Card>, CardValidation>();
builder.Services.AddScoped<IValidator<Module>, ModuleValidation>();
builder.Services.AddScoped<IValidator<Lesson>, LessonValidation>();
builder.Services.AddScoped<CardCategoryQueryParamValidation>();
builder.Services.AddScoped<CardQueryParamValidation>();
builder.Services.AddScoped<ModuleQueryParamValidation>();
builder.Services.AddScoped<LessonQueryParamValidation>();

// Register API logging
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath |
                            HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
});

// Register ProblemDetails 
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
