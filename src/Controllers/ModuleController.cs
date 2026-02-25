using AutoMapper;
using KidFit.Models;
using KidFit.Services;
using KidFit.Shared.Constants;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using KidFit.Shared.Util;
using KidFit.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace KidFit.Controllers
{
    public class ModuleController(ModuleService service, IMapper mapper, ILogger<ModuleController> logger) : Controller
    {
        private readonly ModuleService _service = service;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<ModuleController> _logger = logger;

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Index(int page, int size, string? orderBy, bool? isAsc)
        {
            var param = new QueryParam<Module>(page, size, orderBy, isAsc);
            var modules = await _service.GetAllModulesAsync(param);
            var resp = _mapper.Map<IPagedList<ModuleViewModel>>(modules);

            // Return partial view with HTMX
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_ModuleTable", resp);
            }

            return View(resp);
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create() => View();

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(CreateModuleViewModel req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData[MessageLevel.LOG.ToString()] = Util.GetModelValidationError(ModelState);
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid request";
                    return RedirectToAction(nameof(Create));
                }

                var module = _mapper.Map<Module>(req);

                if (!await _service.CreateModuleAsync(module))
                {
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to create module";
                    return RedirectToAction(nameof(Create));
                }

                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                TempData[MessageLevel.WARNING.ToString()] = ex.Message;
                return RedirectToAction(nameof(Create));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create module: unexpected error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
            }
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(Guid id)
        {
            try
            {
                var module = await _service.GetModuleAsync(id);
                if (module is null)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Module not found";
                    return RedirectToAction(nameof(Index));
                }
                var resp = _mapper.Map<ModuleViewModel>(module);
                return View(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update module: unexpected error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(Guid id, ModuleViewModel req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData[MessageLevel.LOG.ToString()] = Util.GetModelValidationError(ModelState);
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid request";
                    return RedirectToAction(nameof(Update));
                }

                // Get module from database
                var module = await _service.GetModuleAsync(id);
                if (module is null)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Module not found";
                    return RedirectToAction(nameof(Index));
                }

                // Map from request ViewModel to the fetched entity
                _mapper.Map(req, module);

                // Update module
                if (!await _service.UpdateModuleAsync(module))
                {
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to update module";
                    return RedirectToAction(nameof(Update));
                }

                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                TempData[MessageLevel.WARNING.ToString()] = ex.Message;
                return RedirectToAction(nameof(Update));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update module: unexpected error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (!await _service.DeleteModuleAsync(id))
                {
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to delete module";
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                TempData[MessageLevel.WARNING.ToString()] = "Module not found";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete module: unexpected error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
            }
        }

    }
}
