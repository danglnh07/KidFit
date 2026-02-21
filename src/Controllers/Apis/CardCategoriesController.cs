using KidFit.Services;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers.Apis
{
    [ApiController]
    [Route("api/card-categories")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CardCategoryController(CardCategoryService cardCategoryService,
                                        ILogger<CardCategoryController> logger) : ControllerBase
    {
        private readonly CardCategoryService _cardCategoryService = cardCategoryService;
        private readonly ILogger<CardCategoryController> _logger = logger;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Create()
        {
            throw new NotImplementedException();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Update(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> Delete(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
