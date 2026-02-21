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
                                                 RoleService roleService,
                                                 IUnitOfWork uow,
                                                 IConfiguration config,
                                                 ILogger<DbInitilizer> logger)
        {
            // Wait for database migration to run
            await context.Database.MigrateAsync();

            // Check if any account has been created or not for data seeding
            if (await accountService.CountAccountsAsync(allowInactive: true) != 0)
            {
                logger.LogInformation("Database already have data, skip database seeding");
                return;
            }

            // Create roles. Since this run only one for the entire time, no need to 
            // optimize this process
            List<Role> roles = [Role.ADMIN, Role.STAFF, Role.TEACHER, Role.SCHOOL, Role.PARENT];
            foreach (var role in roles)
            {
                await roleService.CreateApplicationRole(role);
            }

            // Create default admin account
            ApplicationUser account = new()
            {
                Email = config.GetValue<string>("AppSettings:DefaultAdminEmail") ?? "admin@kidfit.com",
                UserName = "admin",
                FullName = "Admin",
                IsActive = true,
                AvatarUrl = config.GetValue<string>("AppSettings:DefaultAdminAvatarUrl") ?? "",
            };
            await accountService.CreateAccountAsync(account, Role.ADMIN);

            // Create default card category
            var categories = new List<CardCategory>
            {
                new() {Name = "Group A - ???", Description = "???", BorderColor = "Green"},
                new() {Name = "Group B - ???", Description = "???", BorderColor = "Orange"},
                new() {Name = "Group C - ???", Description = "???", BorderColor = "Blue"},
            };
            await uow.Repo<CardCategory>().CreateBatchAsync(categories);

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
            await uow.Repo<Module>().CreateBatchAsync(modules);

            // Save changes to database
            var success = await uow.SaveChangesAsync();
            if (success != categories.Count + modules.Count)
            {
                logger.LogWarning($"Some initial data is not seeded properly (expected: {categories.Count + modules.Count}, actual: {success})");
                throw new Exception("Failed to seed data");
            }

            logger.LogInformation("Database seeding completed");
        }
    }
}
