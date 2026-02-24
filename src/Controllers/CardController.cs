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
    public class CardController(CardService cardService,
                                CardCategoryService cardCategoryService,
                                IMapper mapper,
                                ILogger<CardController> logger) : Controller
    {
        private readonly CardService _cardService = cardService;
        private readonly CardCategoryService _cardCategoryService = cardCategoryService;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<CardController> _logger = logger;

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Index(int page, int size, string? orderBy, bool? isAsc)
        {
            var param = new QueryParam<Card>(page, size, orderBy, isAsc);
            var cards = await _cardService.GetAllCardsAsync(param, true);
            var resp = _mapper.Map<IPagedList<CardViewModel>>(cards);

            if (Request.Headers.ContainsKey("HX-Request"))
            {
                return PartialView("_CardTable", resp);
            }

            return View(resp);
        }

        [Authorize]
        public async Task<IActionResult> Detail(Guid id)
        {
            var card = await _cardService.GetCardAsync(id, true);
            if (card is null)
            {
                // Should be 404 error page
                return RedirectToAction("Error", "Error");
            }
            var resp = _mapper.Map<CardViewModel>(card);
            return View(resp);
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create()
        {
            // Get the list of all categories
            var categories = await _cardCategoryService.GetAllCardCategoriesWithNameOnlyAsync();

            // If no categories, redirect to Index page -> can only create card from existing category
            if (categories.Count() == 0)
            {
                TempData[MessageLevel.WARNING.ToString()] = "No category available. All card must belong to an existing category";
                return RedirectToAction("Index", "Card");
            }

            _logger.LogInformation($"{categories.Count()}");

            IEnumerable<CategoryOption> availaibles = [];
            foreach (var (id, name) in categories)
            {
                availaibles = availaibles.Append(new CategoryOption()
                {
                    Id = id,
                    Name = name
                });
            }

            _logger.LogInformation($"{availaibles.Count()}");

            // Return to View with categories
            var resp = new CreateCardViewModel()
            {
                AvailableCategories = availaibles,
            };

            _logger.LogInformation($"{resp.AvailableCategories.Count()}");

            return View(resp);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(CreateCardViewModel req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Where(x => x.Value?.Errors.Count > 0).ToDictionary(
                        kvp => kvp.Key, // The field name (e.g., "Email" or "Password")
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray() // Array of error messages for that field
                    );

                    foreach (var error in errors)
                    {
                        _logger.LogWarning($"Field: {error.Key}, Error: {string.Join(", ", error.Value)}");
                    }

                    TempData[MessageLevel.WARNING.ToString()] = "Invalid request";
                    return RedirectToAction("Create", "Card");
                }

                var card = _mapper.Map<Card>(req);
                var result = await _cardService.CreateCardAsync(card);
                if (!result)
                {
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to create card";
                    return RedirectToAction("Create", "Card");
                }

                return RedirectToAction("Index", "Card");
            }
            catch (ValidationException ex)
            {
                TempData[MessageLevel.WARNING.ToString()] = ex.Message;
                return RedirectToAction("Create", "Card");
            }
            catch (DependentEntityNotFoundException ex)
            {
                // This may be possible even through UI, for example user A
                // go to Create page -> already fetch the categories list,
                // then hang the web for a while, in the mean time user B delete a category,
                // then user A create card with that already deleted category wihtout 
                // refreshing the page -> error
                TempData[MessageLevel.ERROR.ToString()] = ex.Message;
                return RedirectToAction("Create", "Card");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create card: {ex.Message}");
                return RedirectToAction("Error", "Error");
            }
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(Guid id)
        {
            try
            {
                // Get card by ID
                var card = await _cardService.GetCardAsync(id, true);

                if (card is null)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Card not found";
                    return RedirectToAction("Index", "Card");
                }

                // Map Model to ViewModel
                var resp = _mapper.Map<UpdateCardViewModel>(card);

                // Get the list of all categories available
                // There should be one category left, since we can still fetch this card, 
                // so no need to check for empty here
                var categories = await _cardCategoryService.GetAllCardCategoriesWithNameOnlyAsync();
                IEnumerable<CategoryOption> availaibles = [];
                foreach (var (categoryId, name) in categories)
                {
                    availaibles = availaibles.Append(new CategoryOption()
                    {
                        Id = categoryId,
                        Name = name
                    });
                }
                resp.AvailableCategories = availaibles;

                return View(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update card: {ex.Message}");
                return RedirectToAction("Error", "Error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(Guid id, UpdateCardViewModel req)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid request";
                    return RedirectToAction("Update", "Card");
                }

                // Get card from database
                var card = await _cardService.GetCardAsync(id);
                if (card is null)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Card not found";
                    return RedirectToAction("Index", "Card");
                }

                _mapper.Map(req, card);

                if (!await _cardService.UpdateCardAsync(card))
                {
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to update card";
                    return RedirectToAction("Update", "Card");
                }

                return RedirectToAction("Index", "Card");
            }
            catch (ValidationException ex)
            {
                TempData[MessageLevel.WARNING.ToString()] = ex.Message;
                return RedirectToAction("Update", "Card");
            }
            catch (DependentEntityNotFoundException ex)
            {
                TempData[MessageLevel.WARNING.ToString()] = ex.Message;
                return RedirectToAction("Update", "Card");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update card: {ex.Message}");
                return RedirectToAction("Error", "Error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                if (!await _cardService.DeleteCardAsync(id))
                {
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to delete card";
                    return RedirectToAction("Index", "Card");
                }

                return RedirectToAction("Index", "Card");
            }
            catch (NotFoundException)
            {
                TempData[MessageLevel.WARNING.ToString()] = "Card not found";
                return RedirectToAction("Index", "Card");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete card: {ex.Message}");
                return RedirectToAction("Error", "Error");
            }
        }
    }

}
