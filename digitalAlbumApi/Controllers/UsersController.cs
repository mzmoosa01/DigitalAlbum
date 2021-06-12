using AutoMapper;
using digitalAlbumApi.DTOs.UserDtos;
using digitalAlbumApi.Helpers;
using digitalAlbumApi.Interfaces;
using digitalAlbumApi.Models;
using digitalAlbumApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitalAlbumApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController(IUserService userService, IMapper mapper, AppSettings appSettings)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] CreateUserDto model)
        {
            var user = _mapper.Map<User>(model);

            try
            {
                _userService.CreateUser(user, model.Password);
                return Ok();
            }
            catch(AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



    }
}
