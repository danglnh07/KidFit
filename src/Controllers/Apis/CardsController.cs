using KidFit.Dtos;
using KidFit.Services;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers.Apis
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CardController(CardService cardService, ILogger<CardController> logger) : ControllerBase
    {
        private readonly CardService _cardService = cardService;
        private readonly ILogger<CardController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryParamDto queryParam)
        {
            try
            {
                var result = await _cardService.GetAllCards(queryParam);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting cards: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _cardService.GetCard(id);
                if (result == null)
                {
                    return NotFound(new { message = $"Card {id} not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting card {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Create([FromBody] CreateCardDto request)
        {
            try
            {
                var result = await _cardService.CreateCard(request);
                return Ok(new { success = result, message = "Card created successfully" });
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
                _logger.LogError($"Error creating card: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCardDto request)
        {
            try
            {
                var result = await _cardService.UpdateCard(id, request);
                return Ok(new { success = result, message = "Card updated successfully" });
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
                _logger.LogError($"Error updating card {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _cardService.DeleteCard(id);
                return Ok(new { success = result, message = "Card deleted successfully" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting card {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
