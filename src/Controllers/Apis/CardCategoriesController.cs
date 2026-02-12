using KidFit.Dtos;
using KidFit.Services;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers.Apis
{
    [ApiController]
    [Route("api/card-categories")]
    [Authorize]
    public class CardCategoryController(CardCategoryService cardCategoryService,
                                        ILogger<CardCategoryController> logger) : ControllerBase
    {
        private readonly CardCategoryService _cardCategoryService = cardCategoryService;
        private readonly ILogger<CardCategoryController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryParamDto queryParam)
        {
            try
            {
                var result = await _cardCategoryService.GetAllCardCategories(queryParam);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting card categories: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _cardCategoryService.GetCardCategory(id);
                if (result is null)
                {
                    return NotFound(new { message = $"Card category {id} not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting card category {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Create([FromBody] CreateCardCategoryDto request)
        {
            try
            {
                var result = await _cardCategoryService.CreateCardCategory(request);
                return Ok(new { success = result, message = "Card category created successfully" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating card category: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCardCategoryDto request)
        {
            try
            {
                var result = await _cardCategoryService.UpdateCardCategory(id, request);
                return Ok(new { success = result, message = "Card category updated successfully" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating card category {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _cardCategoryService.DeleteCardCategory(id);
                return Ok(new { success = result, message = "Card category deleted successfully" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting card category {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
