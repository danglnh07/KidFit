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
            var categories = await _cardCategoryService.GetAllCardCategories(param);
            var resp = _mapper.Map<IPagedList<CardCategoryViewModel>>(categories);
            return View(resp);
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(CreateCardCategoryViewModel req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid request";
                    return RedirectToAction("Create", "CardCategory");
                }

                var category = _mapper.Map<CardCategory>(req);
                if (!await _cardCategoryService.CreateCardCategory(category))
                {
                    // If failed, it would be because the dbcontext couldn't
                    // save changes -> db level error -> should be error not warning
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to create card category";
                    return RedirectToAction("Create", "CardCategory");
                }
                return RedirectToAction("Index", "CardCategory");
            }
            catch (ValidationException ex)
            {
                TempData[MessageLevel.WARNING.ToString()] = ex.Message;
                return RedirectToAction("Create", "CardCategory");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create card category: {ex.Message}");
                return RedirectToAction("Error", "Error");
            }
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(Guid id)
        {
            try
            {
                var category = await _cardCategoryService.GetCardCategory(id);
                if (category is null)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Card category not found";
                    return RedirectToAction("Index", "CardCategory");
                }

                var resp = _mapper.Map<CardCategoryViewModel>(category);
                return View(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update card category: {ex.Message}");
                return RedirectToAction("Error", "Error");
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
                    return RedirectToAction("Update", "CardCategory");
                }

                // Get card category from database
                var category = await _cardCategoryService.GetCardCategory(id);
                if (category is null)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Card category not found";
                    return RedirectToAction("Index", "CardCategory");
                }

                // Map from request ViewModel to the fetched entity
                _mapper.Map(req, category);

                // Update card category
                if (!await _cardCategoryService.UpdateCardCategory(category))
                {
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to update card category";
                    return RedirectToAction("Update", "CardCategory");
                }

                return RedirectToAction("Index", "CardCategory");
            }
            catch (ValidationException ex)
            {
                TempData[MessageLevel.WARNING.ToString()] = ex.Message;
                return RedirectToAction("Update", "CardCategory");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update card category: {ex.Message}");
                return RedirectToAction("Error", "Error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (!await _cardCategoryService.DeleteCardCategory(id))
                {
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to delete card category";
                    return RedirectToAction("Index", "CardCategory");
                }
                return RedirectToAction("Index", "CardCategory");
            }
            catch (NotFoundException)
            {
                TempData[MessageLevel.WARNING.ToString()] = "Card category not found";
                return RedirectToAction("Index", "CardCategory");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete card category: {ex.Message}");
                return RedirectToAction("Error", "Error");
            }
        }
    }
}
