using KidFit.Dtos;
using KidFit.Dtos.Requests;
using KidFit.Services;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers.Views
{
    public class AccountController(AccountService accountService, ILogger<AccountController> logger) : Controller
    {
        private readonly AccountService _accountService = accountService;
        private readonly ILogger<AccountController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAccountDto request)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
