using KidFit.Dtos;
using KidFit.Services;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers.Apis
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LessonController(LessonService lessonService, ILogger<LessonController> logger) : ControllerBase
    {
        private readonly LessonService _lessonService = lessonService;
        private readonly ILogger<LessonController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryParamDto queryParam)
        {
            try
            {
                var result = await _lessonService.GetAllLessons(queryParam);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting lessons: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _lessonService.GetLesson(id);
                if (result == null)
                {
                    return NotFound(new { message = $"Lesson {id} not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting lesson {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Create([FromBody] CreateLessonDto request)
        {
            try
            {
                var result = await _lessonService.CreateLesson(request);
                return Ok(new { success = result, message = "Lesson created successfully" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DependentEntityNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating lesson: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonDto request)
        {
            try
            {
                var result = await _lessonService.UpdateLesson(id, request);
                return Ok(new { success = result, message = "Lesson updated successfully" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DependentEntityNotFoundException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating lesson {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _lessonService.DeleteLesson(id);
                return Ok(new { success = result, message = "Lesson deleted successfully" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting lesson {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
