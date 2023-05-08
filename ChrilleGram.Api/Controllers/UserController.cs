using ChrilleGram.Api.Data;
using ChrilleGram.Api.Interfaces;
using ChrilleGram.Api.Models;
using ChrilleGram.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ChrilleGram.Api.Controllers
{
    [ApiController]
    public class UserController : Controller
    {
        private readonly Context _context;
        private readonly IUserService _userService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public UserController(UserManager<IdentityUser> userManager,
        Context context,
        SignInManager<IdentityUser> signInManager,
        IUserService userService)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _userService = userService;
        }

        [HttpPost("[controller]/[action]")]
        public async Task<IActionResult> CreateUser(string email, string userName, string password)
        {
            var passWordCheck = password.Any(x => char.IsLetter(x));
            if (_userManager.FindByEmailAsync(email).Result != null)
                return BadRequest("User already exists");
            if (!Regex.IsMatch(userName, @"^[a-zA-Z]+$"))
                return BadRequest("Please enter a valid username");
            if (!passWordCheck)
                return BadRequest("Password must contain letters");

            var user = new IdentityUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true
            };

            var created = await _userManager.CreateAsync(user, password);

            await _context.SaveChangesAsync();

            if(created.Succeeded)
            {
                await _context.SaveChangesAsync();
                return Ok("User created");
            } else
            {
                return BadRequest("Something went wrong, please check the error message and contact the support: " + created.Errors);
            }
            
        }

        [HttpPost("[controller]/[action]")]
        public async Task<IActionResult> Authenticate([FromBody] LoginRequest request)
        {
            try
            {
                var auth = await _userService.AuthenticateUserAsync(request.Email, request.Password);
                if (auth != null)
                {
                    var jwt = _userService.Generate(auth);
                    return Ok(auth);
                } else
                {
                    return BadRequest("User does not exist");
                }
            } catch(Exception ex)
            {
                throw ex;
            }
        }

        
    }
}
