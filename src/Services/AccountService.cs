using FluentValidation;
using KidFit.Models;
using KidFit.Shared.Constants;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using KidFit.Shared.TaskRequests;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TickerQ.Utilities;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;
using X.PagedList;

namespace KidFit.Services
{
    public class AccountService(UserManager<ApplicationUser> userManager,
                                IValidator<ApplicationUser> accountValidator,
                                IValidator<QueryParam<ApplicationUser>> queryParamValidator,
                                ITimeTickerManager<TimeTickerEntity> scheduler)
    {
        // Dependencies
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IValidator<ApplicationUser> _accountValidator = accountValidator;
        private readonly IValidator<QueryParam<ApplicationUser>> _queryParamValidator = queryParamValidator;
        private readonly ITimeTickerManager<TimeTickerEntity> _scheduler = scheduler;

        // Task configs
        private readonly int taskExecTimeInSecs = 10;
        private readonly int taskRetries = 3;
        private readonly int[] taskRetryIntervals = [60, 300, 900];

        public async Task CreateAccountAsync(ApplicationUser account, Role role)
        {
            // Validate account
            var validationResult = _accountValidator.Validate(account);
            if (!validationResult.IsValid)
            {
                var message = "Failed to create account: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Use user manager to create account without password
            var createAccResult = await _userManager.CreateAsync(account);
            if (!createAccResult.Succeeded)
            {
                throw IdentityException.Create("Create account failed", createAccResult.Errors);
            }

            // Assign role to user
            var assignRoleResult = await _userManager.AddToRoleAsync(account, role.ToString());
            if (!assignRoleResult.Succeeded)
            {
                throw IdentityException.Create("Create acount failed", assignRoleResult.Errors);
            }

            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(account);

            // Schedule task: send welcome email
            await _scheduler.AddAsync(new TimeTickerEntity
            {
                Function = "SendWelcomeEmail",
                ExecutionTime = DateTime.UtcNow.AddSeconds(taskExecTimeInSecs),
                Request = TickerHelper.CreateTickerRequest(new SendWelcomeEmailRequest(account, resetToken)),
                Description = $"Send welcome email to {account.Email}",
                Retries = taskRetries,
                RetryIntervals = taskRetryIntervals,
            });
        }

        public async Task<ApplicationUser?> GetAccountById(string id, bool allowInactive = false)
        {
            var account = await _userManager.FindByIdAsync(id);
            if (allowInactive) return account;
            return account is null || !account.IsActive ? null : account;
        }

        public async Task<Role> GetRoleByAccount(ApplicationUser account)
        {
            var roles = await _userManager.GetRolesAsync(account);
            Enum.TryParse(roles.FirstOrDefault(), out Role role);
            return role;
        }

        public async Task<IPagedList<ApplicationUser>> GetAllAccounts(QueryParam<ApplicationUser> param, bool allowInactive = false)
        {
            // Validation against query param
            var queryParamValidationResult = _queryParamValidator.Validate(param);
            if (!queryParamValidationResult.IsValid)
            {
                var message = "Failed to get all accounts: query param validation failed";
                List<string> errors = [.. queryParamValidationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Build the query
            var query = _userManager.Users.AsQueryable();

            if (!allowInactive)
            {
                query = query.Where(u => u.IsActive);
            }

            if (param.OrderBy is not null && param.IsAsc is not null)
            {
                query = param.IsAsc == true
                    ? query.OrderBy(param.OrderBy)
                    : query.OrderByDescending(param.OrderBy);
            }

            // Get all accounts 
            return await query.ToPagedListAsync(param.Page, param.Size);
        }

        // This method will ignore password regardless of its value
        // Please use the ChangePassword() method for password update
        // Same case with deactive/activate account
        public async Task<ApplicationUser> UpdateAccount(string id, ApplicationUser req)
        {
            // Get entity from database by ID
            var account = await _userManager.FindByIdAsync(id);

            if (account is null)
            {
                throw NotFoundException.Create(typeof(ApplicationUser).Name);
            }
            else if (!account.IsActive)
            {
                // Even if this method is called by admin -> not allowed
                throw ForbiddenException.Create("Update account failed: account is inactive");
            }

            // Update username
            if (!string.IsNullOrEmpty(req.UserName))
            {
                var result = await _userManager.SetUserNameAsync(account, req.UserName);
                if (!result.Succeeded)
                {
                    throw IdentityException.Create("Update account failed: failed to set username", result.Errors);
                }
            }

            // Update email
            if (!string.IsNullOrEmpty(req.Email))
            {
                var result = await _userManager.SetEmailAsync(account, req.Email);
                if (!result.Succeeded)
                {
                    throw IdentityException.Create("Update account failed: failed to set email", result.Errors);
                }
            }

            // Update other custom fields
            if (!string.IsNullOrEmpty(req.AvatarUrl))
            {
                account.AvatarUrl = req.AvatarUrl;
            }

            if (!string.IsNullOrEmpty(req.FullName))
            {
                account.FullName = req.FullName;
            }

            account.TimeUpdated = DateTimeOffset.UtcNow;

            // Update account
            var res = await _userManager.UpdateAsync(account);
            if (!res.Succeeded)
            {
                throw IdentityException.Create("Update acount failed", res.Errors);
            }

            return account;
        }

        public async Task DeactivateAccount(string id)
        {
            // Get account by ID
            var account = await _userManager.FindByIdAsync(id) ?? throw NotFoundException.Create(typeof(ApplicationUser).Name);
            if (!account.IsActive) return;

            // Update IsActive to false
            account.IsActive = false;
            account.TimeUpdated = DateTimeOffset.UtcNow;
            var result = await _userManager.UpdateAsync(account);
            if (!result.Succeeded)
            {
                throw IdentityException.Create("Deactivate account failed", result.Errors);
            }
        }

        public async Task ActivateAccount(string id)
        {
            // Get account by ID
            var account = await _userManager.FindByIdAsync(id) ?? throw NotFoundException.Create(typeof(ApplicationUser).Name);
            if (account.IsActive) return;

            // Update IsActive to true
            account.IsActive = true;
            account.TimeUpdated = DateTimeOffset.UtcNow;
            var result = await _userManager.UpdateAsync(account);
            if (!result.Succeeded)
            {
                throw IdentityException.Create("Activate account failed", result.Errors);
            }
        }

        public async Task ChangePassword(string id, string oldPassword, string newPassword)
        {
            // Get account by ID
            var account = await _userManager.FindByIdAsync(id);
            if (account is null)
            {
                throw NotFoundException.Create(typeof(ApplicationUser).Name);
            }
            else if (!account.IsActive)
            {
                // Even if this method is called by admin -> not allowed
                throw ForbiddenException.Create("Change password failed: account is inactive");
            }

            // Update password
            var result = await _userManager.ChangePasswordAsync(account, oldPassword, newPassword);
            if (!result.Succeeded)
            {
                throw IdentityException.Create("Change password failed", result.Errors);
            }

            // Record changes with TimeUpdated
            account.TimeUpdated = DateTimeOffset.UtcNow;
            var res = await _userManager.UpdateAsync(account);
            if (!res.Succeeded)
            {
                throw IdentityException.Create("Change password failed", res.Errors);
            }
        }

        public async Task<int> CountAccountsAsync(bool allowInactive = false)
        {
            var query = _userManager.Users.AsQueryable();
            if (!allowInactive) query = query.Where(u => u.IsActive);
            return await query.CountAsync();
        }
    }
}
