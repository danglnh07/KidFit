using AutoMapper;
using KidFit.Models;
using KidFit.Services;
using KidFit.Shared.Constants;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using KidFit.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

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
            // To avoid N + 1 query, we won't fetch accounts' roles in this method,
            // since UserManager and RoleManager doesn't allow for prefetching,
            // and work-around solution is too cumbersome to implement.

            // Create query parameter
            var param = new QueryParam<ApplicationUser>(page, size, orderBy, isAsc);

            // Get all accounts (including inactive, since this is admin dashboard)
            var accounts = await _accountService.GetAllAccounts(param, true);

            // Map from model to view model
            var resp = _mapper.Map<IPagedList<AccountViewModel>>(accounts);

            return View(resp);
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Detail(string id)
        {
            // Get account by ID
            var account = await _accountService.GetAccountById(id);
            if (account is null)
            {
                TempData[MessageLevel.WARNING.ToString()] = "Account not found";
                return RedirectToAction("Index", "Account");
            }

            // Get account's role 
            var role = await _accountService.GetRoleByAccount(account);

            // Map models to view model
            var resp = _mapper.Map<AccountViewModelWithRole>(account);
            resp.Role = role.ToString();

            return View(resp);
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create() => View();

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(CreateAccountViewModel req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid request";
                    return RedirectToAction("Create", "Account");
                }

                // Try parse role
                if (!Enum.TryParse(req.Role, out Role role))
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid role value";
                    return RedirectToAction("Create", "Account");
                }

                // Create account model
                var account = _mapper.Map<ApplicationUser>(req);

                // Create account
                await _accountService.CreateAccountAsync(account, role);
                TempData[MessageLevel.SUCCESS.ToString()] = "Account created successfully";
                return RedirectToAction("Index", "Account");
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Failed to create account: {ex.Message}");
                TempData[MessageLevel.ERROR.ToString()] = ex.Message;
                return RedirectToAction("Create", "Account");
            }
            catch (Exception ex)
            {
                // Since this is unexpected error, we'll redirect to a dedicated error page 
                // without any detail error message
                _logger.LogError($"Failed to create account: {ex.Message}");
                return RedirectToAction("Error", "Error");
            }
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(string id)
        {
            // Get account by ID
            var account = await _accountService.GetAccountById(id);
            if (account is null)
            {
                TempData[MessageLevel.WARNING.ToString()] = "Account not found";
                return RedirectToAction("Index", "Account");
            }

            // Map from model to view model
            var resp = _mapper.Map<AccountViewModel>(account);

            return View(resp);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(string id, AccountViewModel req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid request";
                    return RedirectToAction("Update", "Account");
                }

                // Map from request ViewModel to model 
                var account = _mapper.Map<ApplicationUser>(req);

                // Update account
                await _accountService.UpdateAccount(id, account);

                // Redirect to index page
                TempData[MessageLevel.SUCCESS.ToString()] = "Account updated successfully";
                return RedirectToAction("Index", "Account");
            }
            catch (NotFoundException)
            {
                TempData[MessageLevel.WARNING.ToString()] = "Account not found";
                return RedirectToAction("Index", "Account");
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Failed to update account: {ex.Message}");
                TempData[MessageLevel.ERROR.ToString()] = ex.Message;
                return RedirectToAction("Update", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update account: {ex.Message}");
                return RedirectToAction("Error", "Error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Deactivate(string id)
        {
            try
            {
                // Deactivate account
                await _accountService.DeactivateAccount(id);

                // Redirect to index page
                TempData[MessageLevel.SUCCESS.ToString()] = "Account deactivated";
                return RedirectToAction("Index", "Account");
            }
            catch (NotFoundException)
            {
                TempData[MessageLevel.WARNING.ToString()] = "Account not found";
                return RedirectToAction("Index", "Account");
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Failed to deactivate account: {ex.Message}");
                TempData[MessageLevel.ERROR.ToString()] = ex.Message;
                return RedirectToAction("Index", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to deactivate account: {ex.Message}");
                return RedirectToAction("Error", "Error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Activate(string id)
        {
            try
            {
                // Activate account
                await _accountService.ActivateAccount(id);

                // Redirect to index page
                TempData[MessageLevel.SUCCESS.ToString()] = "Account activated";
                return RedirectToAction("Index", "Account");
            }
            catch (NotFoundException)
            {
                TempData[MessageLevel.WARNING.ToString()] = "Account not found";
                return RedirectToAction("Index", "Account");
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Failed to activate account: {ex.Message}");
                TempData[MessageLevel.ERROR.ToString()] = ex.Message;
                return RedirectToAction("Index", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to activate account: {ex.Message}");
                return RedirectToAction("Error", "Error");
            }
        }
    }
}
