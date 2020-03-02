using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParkyAPI.Models;
using ParkyAPI.Repository;

namespace ParkyAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "ParkyOpenAPISpecUsers")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;

        public UsersController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] AuthUser model) 
        {
            //see if user exists when client calls
            var user = _userRepo.Authenticate(model.Username, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect." });

            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register([FromBody] AuthUser model)
        {
            if (!_userRepo.IsUniqueUser(model.Username))
                return BadRequest(new { message = "Username already exists" });

            var user = _userRepo.Register(model.Username, model.Password);

            if (user == null)
                return BadRequest(new { message = "Error while registering..." });

            return Ok();
        }

    }
}