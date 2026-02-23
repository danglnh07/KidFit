using KidFit.Shared.Constants;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace KidFit.Services
{
    public class RoleService(RoleManager<IdentityRole> roleManager, ILogger<RoleService> logger)
    {
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly ILogger<RoleService> _logger = logger;

        public async Task CreateApplicationRoleAsync(Role role)
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

        public async Task<bool> DeleteRoleAsync(Role role)
        {
            // Check if role exists
            if (!await _roleManager.RoleExistsAsync(role.ToString()))
            {
                throw NotFoundException.Create(typeof(IdentityRole).Name);
            }

            // Delete role
            var result = await _roleManager.DeleteAsync(new IdentityRole(role.ToString()));
            if (!result.Succeeded || result.Errors.Count() != 0)
            {
                throw IdentityException.Create("Failed to delete role", result.Errors);
            }

            return true;
        }
    }
}
