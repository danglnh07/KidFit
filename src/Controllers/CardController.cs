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
    public class CardController(CardService cardService,
                                CardCategoryService cardCategoryService,
                                LinkGenerator _linkGenerator,
                                IMapper mapper,
                                ILogger<CardController> logger) : Controller
    {
        private readonly CardService _cardService = cardService;
        private readonly CardCategoryService _cardCategoryService = cardCategoryService;
        private readonly LinkGenerator _linkGenerator = _linkGenerator;
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
                // Admin dashboard detail and other role page use the same view,
                // so we should throw a 404 page instead of redirect to any other pages
                // return RedirectToAction("NotFoundPage", "Error");
                return RedirectToAction(nameof(ErrorController.NotFoundPage), nameof(ErrorController).Replace("Controller", string.Empty));
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
                return RedirectToAction(nameof(Index));
            }

            IEnumerable<CategoryOption> availaibles = [];
            foreach (var (id, name) in categories)
            {
                availaibles = availaibles.Append(new CategoryOption(id, name));
            }

            // Return to View with categories
            var resp = new CreateCardViewModel()
            {
                AvailableCategories = availaibles,
            };

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
                    TempData[MessageLevel.LOG.ToString()] = Util.GetModelValidationError(ModelState);
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid request";
                    return RedirectToAction(nameof(Create));
                }

                var card = _mapper.Map<Card>(req);
                var result = await _cardService.CreateCardAsync(card);
                if (!result)
                {
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to create card";
                    return RedirectToAction(nameof(Create));
                }

                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                TempData[MessageLevel.WARNING.ToString()] = ex.Message;
                return RedirectToAction(nameof(Create));
            }
            catch (DependentEntityNotFoundException ex)
            {
                // This may be possible even through UI, for example user A
                // go to Create page -> already fetch the categories list,
                // then hang the web for a while, in the mean time user B delete a category,
                // then user A create card with that already deleted category wihtout
                // refreshing the page -> error
                TempData[MessageLevel.ERROR.ToString()] = ex.Message;
                return RedirectToAction(nameof(Create));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create card: unexpected error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
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
                    return RedirectToAction(nameof(Index));
                }

                // Map Model to ViewModel
                var resp = _mapper.Map<UpdateCardViewModel>(card);

                // Get the list of all categories available
                // There should be one category left (if service behave correctly),
                // since we can still fetch this card, so no need to check for empty here
                var categories = await _cardCategoryService.GetAllCardCategoriesWithNameOnlyAsync();
                IEnumerable<CategoryOption> availaibles = [];
                foreach (var (categoryId, name) in categories)
                {
                    availaibles = availaibles.Append(new CategoryOption(id, name));
                }
                resp.AvailableCategories = availaibles;

                return View(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update card: unexpected error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
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
                    TempData[MessageLevel.LOG.ToString()] = Util.GetModelValidationError(ModelState);
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid request";
                    return RedirectToAction(nameof(Update));
                }

                // Get card from database
                var card = await _cardService.GetCardAsync(id);
                if (card is null)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Card not found";
                    return RedirectToAction(nameof(Index));
                }

                _mapper.Map(req, card);

                if (!await _cardService.UpdateCardAsync(card))
                {
                    TempData[MessageLevel.ERROR.ToString()] = "Failed to update card";
                    return RedirectToAction(nameof(Update));
                }

                return RedirectToAction(nameof(Index));
            }
            catch (ValidationException ex)
            {
                TempData[MessageLevel.WARNING.ToString()] = ex.Message;
                return RedirectToAction(nameof(Update));
            }
            catch (DependentEntityNotFoundException ex)
            {
                TempData[MessageLevel.WARNING.ToString()] = ex.Message;
                return RedirectToAction(nameof(Update));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to update card: unexpected error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
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
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                TempData[MessageLevel.WARNING.ToString()] = "Card not found";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to delete card: unexpected error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
            }
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetCardQr(Guid id, bool? autoDownload)
        {
            try
            {
                // Check id card with this ID exists
                var card = await _cardService.GetCardAsync(id, false);
                if (card is null)
                {
                    TempData[MessageLevel.WARNING.ToString()] = "Card not found";
                    return RedirectToAction(nameof(Index));
                }

                // Build the Url
                var url = _linkGenerator.GetUriByAction(HttpContext, nameof(Detail), nameof(CardController), new { id });

                // Build the qr
                var qr = QrService.GenerateQr(url!);

                // If no download otpion, just stream image back 
                if (autoDownload is null || autoDownload == false)
                {
                    return File(qr, "image/png");
                }

                // Create filename from card name
                var filename = $"{card.Name}.png";

                // Stream with auto download
                return File(qr, "image/png", filename);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to generate QR code: unexpected error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
            }
        }
    }

}
