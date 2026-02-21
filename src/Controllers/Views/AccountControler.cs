using KidFit.Services;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers.Views
{
    public class AccountController(AccountService accountService, ILogger<AccountController> logger) : Controller
    {
        private readonly AccountService _accountService = accountService;
        private readonly ILogger<AccountController> _logger = logger;

    }
}
