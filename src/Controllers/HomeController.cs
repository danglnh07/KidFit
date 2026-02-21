using System.Security.Claims;
using KidFit.Models;
using KidFit.Repositories;
using KidFit.Services;
using KidFit.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers
{
    public class HomeController(AccountService accountService,
                                IUnitOfWork uow,
                                ILogger<HomeController> logger) : Controller
    {
        private readonly AccountService _accountService = accountService;
        private readonly IUnitOfWork _uow = uow;
        private readonly ILogger<HomeController> _logger = logger;

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Dashboard()
        {
            // Get user ID from claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                _logger.LogError("Cannot get user ID from claims");
                TempData["Message"] = "An error occurred";
                return Redirect("/auth/login");
            }

            // Get user infomation
            var user = await _accountService.GetAccountById(userId);
            if (user is null)
            {
                _logger.LogWarning("account not found");
                TempData["Message"] = "An error occurred";
                return Redirect("/auth/login");
            }

            // Get role 
            var role = await _accountService.GetRoleByAccount(user);


            // Create view model 
            DashboardViewModel viewModel = new()
            {
                Username = user.UserName!,
                Role = role.ToString(),
                AvatarUrl = string.IsNullOrEmpty(user.AvatarUrl) ? "/static/default_avatar.webp" : user.AvatarUrl,
                TotalUsers = await _accountService.CountAccountsAsync(),
                TotalCards = await _uow.Repo<Card>().CountAsync(),
                TotalLessons = await _uow.Repo<Lesson>().CountAsync()
            };

            return View(viewModel);
        }
    }
}
