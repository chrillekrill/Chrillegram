using ChrilleGram.Api.Data;
using ChrilleGram.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ChrilleGram.Api.Controllers
{
    [ApiController]
    public class UserController : Controller
    {
        private readonly Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public UserController(UserManager<IdentityUser> userManager,
        Context context,
        SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
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
                var auth = await AuthenticateUserAsync(request.Email, request.Password);
                if (auth != null)
                {
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

        private async Task<IdentityUser?> AuthenticateUserAsync(string email, string password)
        {
            var currentUser = await _userManager.FindByEmailAsync(email);
            var locked = await _userManager.IsLockedOutAsync(currentUser);

            if(locked)
            {
                return null;
            }

            var result = await _signInManager.PasswordSignInAsync(currentUser, password, false, false);

            if (result.Succeeded)
            {
                return currentUser;
            }
            else
            {
                return null;
            }
        }
    }
}
