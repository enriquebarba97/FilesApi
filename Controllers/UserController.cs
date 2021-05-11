

using System.Collections.Generic;
using AutoMapper;
using FilesApi.Helpers;
using FilesApi.Models;
using FilesApi.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FilesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly UserService _userService;
        private readonly IMapper _mapper;

        public UserController(UserService service, IMapper mapper)
        {
            _userService = service;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var users = _userService.GetUsers();
            if(users == null || users.Count == 0)
                return NoContent();
            return Ok(_userService.GetUsers());
        }

        [HttpGet("{username}")]
        public IActionResult GetUser(string username)
        {
            var user = _userService.GetByUsername(username);

            return Ok(user);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody]UserDto userDto)
        {
            // map dto to entity
            var user = _mapper.Map<User>(userDto);

            try 
            {
                // save 
                _userService.Create(user, userDto.Password);
                return Ok();
            } 
            catch(AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserDto userDto)
        {
            var token = _userService.Authenticate(userDto.Username, userDto.Password);

            if (token == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            // return basic user info (without password) and token to store client side
            return Ok(token);
        }

        [HttpGet("check")]
        public IActionResult CheckLoggedUser()
        {
            var user = HttpContext.User.Identity.Name;
            return Ok(user);
        }
    }
}