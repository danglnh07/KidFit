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
    public class CardCategoryController(CardCategoryService cardCategoryService,
                                        IMapper mapper,
                                        ILogger<CardCategoryController> logger) : Controller
    {
        private readonly CardCategoryService _cardCategoryService = cardCategoryService;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<CardCategoryController> _logger = logger;

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Index(int page, int size, string? orderBy, bool? isAsc)
        {
            var param = new QueryParam<CardCategory>(page, size, orderBy, isAsc);
            var categories = await _cardCategoryService.GetAllCardCategoriesAsync(param);
            var resp = _mapper.Map<IPagedList<CardCategoryViewModel>>(categories);

            // Return partial view with HTMX
            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CardCategoryTable", resp);
            }

            return View(resp);
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create() => View();

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(CreateCardCategoryViewModel req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData[MessageLevel.LOG.ToString()] = Util.GetModelValidationError(ModelState);
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid request";
                    return RedirectToAction(nameof(Create));
                }

                var category = _mapper.Map<CardCategory>(req);
                if (!await _cardCategoryService.CreateCardCategoryAsync(category))
                {
                    // If failed, it would be because the dbcontext couldn't
                    // save changes -> db level error -> message level should be error not warning
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to create card category";
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
                _logger.LogError($"Failed to create card category: unexpeced error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
            }
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(Guid id)
        {
            try
            {
                var category = await _cardCategoryService.GetCardCategoryAsync(id);
                if (category is null)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Card category not found";
                    return RedirectToAction(nameof(Index));
                }

                var resp = _mapper.Map<CardCategoryViewModel>(category);
                return View(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update card category: unexpeced error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(Guid id, CardCategoryViewModel req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid request";
                    return RedirectToAction(nameof(Update));
                }

                // Get card category from database
                var category = await _cardCategoryService.GetCardCategoryAsync(id);
                if (category is null)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Card category not found";
                    return RedirectToAction(nameof(Index));
                }

                // Map from request ViewModel to the fetched entity
                _mapper.Map(req, category);

                // Update card category
                if (!await _cardCategoryService.UpdateCardCategoryAsync(category))
                {
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to update card category";
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
                _logger.LogError($"Failed to update card category: unexpeced error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (!await _cardCategoryService.DeleteCardCategoryAsync(id))
                {
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to delete card category";
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                TempData[MessageLevel.WARNING.ToString()] = "Card category not found";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete card category: unexpeced error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
            }
        }
    }
}
