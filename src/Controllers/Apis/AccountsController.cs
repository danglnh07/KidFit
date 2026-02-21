using System.Security.Claims;
using AutoMapper;
using KidFit.Dtos;
using KidFit.Dtos.Requests;
using KidFit.Dtos.Responses;
using KidFit.Models;
using KidFit.Services;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Queries;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using X.PagedList;

namespace KidFit.Controllers.Apis
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AccountsController(AccountService accountService,
                                    IMapper mapper,
                                    ILogger<AccountsController> logger) : ControllerBase
    {
        private readonly AccountService _accountService = accountService;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<AccountsController> _logger = logger;

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateAccountDto req)
        {
            throw new NotImplementedException();
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetById(string id)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAll()
        {
            throw new NotImplementedException();
        }

        [HttpPatch("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateAccountDto request)
        {
            throw new NotImplementedException();
        }

        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            throw new NotImplementedException();
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateAccountDto request)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Deactivate(string id)
        {
            throw new NotImplementedException();
        }

        // Should be POST or PUT or PATCH ???
        [HttpPost("id")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Activate(string id)
        {
            throw new NotImplementedException();
        }
    }
}
