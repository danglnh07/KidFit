using KidFit.Dtos;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Services;
using KidFit.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace KidFit.Data
{
    public class DbInitilizer
    {
        public static async Task InitializeAsync(AppDbContext context,
                                                 AccountService accountService,
                                                 IUnitOfWork uow,
                                                 IConfiguration config,
                                                 ILogger<DbInitilizer> logger)
        {
            await context.Database.MigrateAsync();

            // We'll use this method to seed admin default account and some data, 
            // so to check if this is truly first time running or not, we just need to check 
            // if there is even any data yet, specifically the ApplicationUser set.
            if (await accountService.AnyAccountExist())
            {
                logger.LogInformation("Skip database seeding");
                return;
            }

            // Create roles
            List<Role> roles = [Role.ADMIN, Role.STAFF, Role.TEACHER, Role.SCHOOL, Role.PARENT];
            foreach (var role in roles)
            {
                await accountService.CreateApplicationRole(role);
            }

            // Create default admin account
            await accountService.CreateAccountAsync(new CreateAccountDto()
            {
                Email = config.GetValue<string>("AppSettings:DefaultAdminEmail") ?? "admin@kidfit.com",
                Username = "admin",
                FullName = "Admin",
                Role = Role.ADMIN,
            });

            // Create default card category
            var categories = new List<CardCategory>
            {
                new() {Name = "Group A - ???", Description = "???", BorderColor = "Green"},
                new() {Name = "Group B - ???", Description = "???", BorderColor = "Orange"},
                new() {Name = "Group C - ???", Description = "???", BorderColor = "Blue"},
            };
            await uow.Repo<CardCategory>().CreateBatch(categories);

            // Create default modules 
            var modules = new List<Module>
            {
                new () {Name = "Module 1", Description = "???", CoreSlot = 35, TotalSlot = 70},
                new () {Name = "Module 2", Description = "???", CoreSlot = 35, TotalSlot = 70},
                new () {Name = "Module 3", Description = "???", CoreSlot = 35, TotalSlot = 70},
                new () {Name = "Module 4", Description = "???", CoreSlot = 35, TotalSlot = 70},
                new () {Name = "Module 5", Description = "???", CoreSlot = 35, TotalSlot = 70},
                new () {Name = "Module 6", Description = "???", CoreSlot = 35, TotalSlot = 70},
            };
            await uow.Repo<Module>().CreateBatch(modules);
            if (await uow.SaveChangesAsync() == 0)
            {
                logger.LogWarning("Failed to seed data");
                throw new Exception("Failed to seed data");
            }

            logger.LogInformation("Database seeding completed");
        }
    }
}
