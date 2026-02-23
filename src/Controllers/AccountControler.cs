using AutoMapper;
using KidFit.Models;
using KidFit.Services;
using KidFit.Shared.Constants;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using KidFit.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers
{
    public class AccountController(AccountService accountService, IMapper mapper, ILogger<AccountController> logger) : Controller
    {
        private readonly AccountService _accountService = accountService;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AccountController> _logger = logger;

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Index(int page, int size, string? orderBy, bool? isAsc)
        {
            var param = new QueryParam<ApplicationUser>(page, size, orderBy, isAsc);

            var accounts = await _accountService.GetAllAccounts(param, true);

            var resp = new AccountsViewModel()
            {
                TotalAccount = accounts.TotalItemCount,
                CurrentPage = accounts.PageNumber,
                PageSize = accounts.PageSize,
                Accounts = _mapper.Map<List<AccountViewModel>>(accounts),
            };

            return View(resp);
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Detail(string id)
        {
            var account = await _accountService.GetAccountById(id);
            if (account is null)
            {
                TempData["Error"] = "Account not found";
                return RedirectToAction("Index", "Account");
            }

            var role = await _accountService.GetRoleByAccount(account);
            var resp = _mapper.Map<AccountViewModelWithRole>(account);
            resp.Role = role.ToString();
            return View(resp);
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(CreateAccountViewModel req)
        {
            try
            {
                // Try parse role 
                if (Enum.TryParse(req.Role, out Role role))
                {
                    TempData["Message"] = "Invalid role";
                }

                // Create account model
                var account = _mapper.Map<ApplicationUser>(req);

                // Create account
                await _accountService.CreateAccountAsync(account, role);
                TempData["Message"] = "Account created successfully";
                return RedirectToAction("Index", "Account");
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Failed to create account: {ex.Message}");
                TempData["Message"] = ex.Message;
                return RedirectToAction("Create", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create account: {ex.Message}");
                TempData["Message"] = "Failed to create account";
                return RedirectToAction("Create", "Account");
            }
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(string id)
        {
            var account = await _accountService.GetAccountById(id);
            if (account is null)
            {
                TempData["Message"] = "Account not found";
                return RedirectToAction("Index", "Account");
            }

            var resp = _mapper.Map<AccountViewModel>(account);
            return View(resp);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(string id, UpdateAccountViewModel req)
        {
            try
            {
                var account = _mapper.Map<ApplicationUser>(req);
                await _accountService.UpdateAccount(id, account);
                TempData["Message"] = "Account updated successfully";
                return RedirectToAction("Index", "Account");
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning($"Failed to update account: {ex.Message}");
                TempData["Error"] = "Account not found";
                return RedirectToAction("Update", "Account");
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Failed to update account: {ex.Message}");
                TempData["Message"] = ex.Message;
                return RedirectToAction("Update", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update account: {ex.Message}");
                TempData["Message"] = "Failed to update account";
                return RedirectToAction("Update", "Account");
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Deactivate(string id)
        {
            try
            {
                await _accountService.DeactivateAccount(id);
                TempData["Success"] = "Account deactivated";
                return RedirectToAction("Index", "Account");
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning($"Failed to deactivate account: {ex.Message}");
                TempData["Error"] = "Account not found";
                return RedirectToAction("Index", "Account");
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Failed to deactivate account: {ex.Message}");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to deactivate account: {ex.Message}");
                TempData["Error"] = "Failed to deactivate account";
                return RedirectToAction("Index", "Account");
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Activate(string id)
        {
            try
            {
                await _accountService.ActivateAccount(id);
                TempData["Success"] = "Account activated";
                return RedirectToAction("Index", "Account");
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning($"Failed to activate account: {ex.Message}");
                TempData["Error"] = "Account not found";
                return RedirectToAction("Index", "Account");
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Failed to activate account: {ex.Message}");
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to activate account: {ex.Message}");
                TempData["Error"] = "Failed to activate account";
                return RedirectToAction("Index", "Account");
            }
        }
    }
}
