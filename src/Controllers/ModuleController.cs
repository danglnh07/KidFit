using KidFit.Dtos.Requests;
using KidFit.Services;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers
{
    [ApiController]
    [Route("api/modules")]
    // [Authorize]
    public class ModuleController(ModuleService moduleService, ILogger<ModuleController> logger) : ControllerBase
    {
        private readonly ModuleService _moduleService = moduleService;
        private readonly ILogger<ModuleController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryParamDto queryParam)
        {
            try
            {
                var result = await _moduleService.GetModules(queryParam);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting modules: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _moduleService.GetModule(id);
                if (result == null)
                {
                    return NotFound(new { message = $"Module {id} not found" });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting module {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPost]
        // [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Create([FromBody] CreateModuleDto request)
        {
            try
            {
                var result = await _moduleService.CreateModule(request);
                return Ok(new { success = result, message = "Module created successfully" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating module: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPut("{id}")]
        // [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateModuleDto request)
        {
            try
            {
                var result = await _moduleService.UpdateModule(id, request);
                return Ok(new { success = result, message = "Module updated successfully" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errors = ex.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating module {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpDelete("{id}")]
        // [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _moduleService.DeleteModule(id);
                return Ok(new { success = result, message = "Module deleted successfully" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting module {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
