using KidFit.Dtos;
using KidFit.Models;
using KidFit.Shared.Constants;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TickerQ.Utilities;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;

namespace KidFit.Services
{
    public class AccountService(UserManager<ApplicationUser> userManager,
                                RoleManager<IdentityRole> roleManager,
                                ITimeTickerManager<TimeTickerEntity> scheduler,
                                ILogger<AccountService> logger)
    {
        // Since we use IdentityUser, UserManager already implement CRUD for us
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly ITimeTickerManager<TimeTickerEntity> _scheduler = scheduler;
        private readonly ILogger<AccountService> _logger = logger;

        // Since we need to fetch role from UserManager, using AutoMapper wouldn't be possible,
        // so we create our own mapper here
        private async Task<ViewAccountDto> MapFromModelAsync(ApplicationUser account)
        {
            var dto = new ViewAccountDto()
            {
                Id = account.Id,
                Username = account.UserName ?? "",
                FullName = account.FullName ?? "",
                Email = account.Email ?? "",
                AvatarUrl = account.AvatarUrl,
            };

            // Get role from account
            var roles = await _userManager.GetRolesAsync(account);
            dto.Role = roles.FirstOrDefault() ?? "";
            return dto;
        }

        public async Task CreateAccountAsync(CreateAccountDto req)
        {
            // Map from DTO to model
            var account = new ApplicationUser()
            {
                Email = req.Email,
                UserName = req.Username,
                FullName = req.FullName,
                IsActive = true,
            };

            // Use user manager to create account without password
            var createAccResult = await _userManager.CreateAsync(account);
            if (!createAccResult.Succeeded || createAccResult.Errors.Count() != 0)
            {
                throw IdentityException.Create("Create acount failed", createAccResult.Errors);
            }

            // Assign role to user
            var assignRoleResult = await _userManager.AddToRoleAsync(account, req.Role.ToString());
            if (!assignRoleResult.Succeeded || assignRoleResult.Errors.Count() != 0)
            {
                throw IdentityException.Create("Create acount failed", assignRoleResult.Errors);
            }

            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(account);

            // Schedule task: send welcome email
            await _scheduler.AddAsync(new TimeTickerEntity
            {
                Function = "SendWelcomeEmail",
                ExecutionTime = DateTime.UtcNow.AddSeconds(10),
                Request = TickerHelper.CreateTickerRequest(new WelcomeEmailRequest()
                {
                    Id = account.Id,
                    Username = account.UserName,
                    Email = account.Email,
                    Fullname = account.FullName,
                    Token = resetToken
                }),
                Description = $"Send welcome email to {account.Email}",
                Retries = 3,
                RetryIntervals = [60, 300, 900],
            });
        }

        public async Task<ViewAccountDto?> GetAccountById(string id)
        {
            var account = await _userManager.FindByIdAsync(id);
            if (account is null) return null;
            return await MapFromModelAsync(account);
        }

        public async Task<ViewAccountDto> UpdateAccount(string id, UpdateAccountDto req)
        {
            // Get entity from database by ID
            var account = await _userManager.FindByIdAsync(id) ?? throw NotFoundException.Create(typeof(ApplicationUser).Name);

            if (req.Username is not null)
            {
                var result = await _userManager.SetUserNameAsync(account, req.Username);
                if (!result.Succeeded || result.Errors.Count() != 0)
                {
                    throw IdentityException.Create("Update account failed: failed to set username", result.Errors);
                }
            }

            if (req.Email is not null)
            {
                var result = await _userManager.SetEmailAsync(account, req.Email);
                if (!result.Succeeded || result.Errors.Count() != 0)
                {
                    throw IdentityException.Create("Update account failed: failed to set email", result.Errors);
                }
            }

            if (req.AvatarUrl is not null)
            {
                account.AvatarUrl = req.AvatarUrl;
            }

            if (req.FullName is not null)
            {
                account.FullName = req.FullName;
            }

            account.TimeUpdated = DateTimeOffset.UtcNow;

            // Update account
            var res = await _userManager.UpdateAsync(account);
            if (!res.Succeeded || res.Errors.Count() != 0)
            {
                throw IdentityException.Create("Update acount failed", res.Errors);
            }

            return await MapFromModelAsync(account);
        }

        public async Task DeactivateAccount(string id)
        {
            // Get account by ID
            var account = await _userManager.FindByIdAsync(id) ?? throw NotFoundException.Create(typeof(ApplicationUser).Name);

            // Update IsActive to false
            account.IsActive = false;
            account.TimeUpdated = DateTimeOffset.UtcNow;
            var result = await _userManager.UpdateAsync(account);
            if (!result.Succeeded || result.Errors.Count() != 0)
            {
                throw IdentityException.Create("Deactivate account failed", result.Errors);
            }
        }

        public async Task ChangePassword(string id, string oldPassword, string newPassword)
        {
            // Get account by ID
            var account = await _userManager.FindByIdAsync(id) ?? throw NotFoundException.Create(typeof(ApplicationUser).Name);

            // Update password
            var result = await _userManager.ChangePasswordAsync(account, oldPassword, newPassword);
            if (!result.Succeeded || result.Errors.Count() != 0)
            {
                throw IdentityException.Create("Change password failed", result.Errors);
            }

            // Record changes with TimeUpdated
            account.TimeUpdated = DateTimeOffset.UtcNow;
            var res = await _userManager.UpdateAsync(account);
            if (!res.Succeeded || res.Errors.Count() != 0)
            {
                throw IdentityException.Create("Change password failed", res.Errors);
            }
        }

        public async Task<bool> AnyAccountExist()
        {
            return await _userManager.Users.AnyAsync();
        }

        public async Task CreateApplicationRole(Role role)
        {
            // Check if role already exists
            if (await _roleManager.RoleExistsAsync(role.ToString()))
            {
                _logger.LogWarning($"Role {role} already exists");
                return;
            }

            // Create role
            var result = await _roleManager.CreateAsync(new IdentityRole(role.ToString()));
            if (!result.Succeeded || result.Errors.Count() != 0)
            {
                throw IdentityException.Create("Failed to create role", result.Errors);
            }
        }
    }
}
