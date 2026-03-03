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
            // Create view model 
            DashboardViewModel viewModel = new()
            {
                TotalUsers = await _accountService.CountAccountsAsync(),
                TotalCards = await _uow.Repo<Card>().CountAsync(),
                TotalLessons = await _uow.Repo<Lesson>().CountAsync()
            };

            return View(viewModel);
        }
    }
}
