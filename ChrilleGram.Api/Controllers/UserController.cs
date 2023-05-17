using ChrilleGram.Api.Data;
using ChrilleGram.Api.Interfaces;
using ChrilleGram.Api.Models;
using ChrilleGram.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.RegularExpressions;

namespace ChrilleGram.Api.Controllers
{
    [ApiController]
    public class UserController : Controller
    {
        private readonly Context _context;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public UserController(UserManager<IdentityUser> userManager,
        Context context,
        SignInManager<IdentityUser> signInManager,
        IUserService userService,
        IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _userService = userService;
            _configuration = configuration;
        }

        [HttpPost("[controller]/[action]")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterRequest register)
        {
            var passWordCheck = register.Password.Any(x => char.IsLetter(x));
            if (await _userManager.FindByEmailAsync(register.Email) != null)
                return BadRequest("Email already in use");
            if (await _userManager.FindByNameAsync(register.Username) != null)
                return BadRequest("Username is already taken");
            if (!Regex.IsMatch(register.Username, @"^[a-zA-Z]+$"))
                return BadRequest("Please enter a valid username");
            if (!passWordCheck)
                return BadRequest("Password must contain letters");

            var user = new IdentityUser
            {
                UserName = register.Username,
                Email = register.Email,
                EmailConfirmed = true
            };

            var created = await _userManager.CreateAsync(user, register.Password);
            await _userManager.AddToRoleAsync(user, "User");

            if(created.Succeeded)
            {
                await _context.SaveChangesAsync();

                var currentUser = await _userManager.FindByEmailAsync(register.Email);

                var jwt = _userService.Generate(currentUser);

                var returnModel = new
                {
                    currentUser,
                    jwt
                };

                await _context.SaveChangesAsync();

                return Ok(returnModel);
            } else
            {
                var errorModel = new
                {
                    ErrorMessage = "Something went wrong, please check the error message and contact the support: " + created.Errors
                };

                return BadRequest(errorModel);
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
                    var jwt = await _userService.Generate(auth);
                    var returnModel = new
                    {
                        auth,
                        jwt
                    };
                    return Ok(returnModel);
                } else
                {
                    return BadRequest("User does not exist");
                }
            } catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost("[controller]/[action]")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] JwtRequest? token)
        {
            try
            {
                var tokenString = "";

                if (string.IsNullOrEmpty(token?.Jwt))
                {
                    tokenString = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                } else
                {
                    tokenString = token.Jwt;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                tokenHandler.ValidateToken(tokenString, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = false // token expiration is validated by the issuer
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "id").Value;
                var user = await _userManager.FindByIdAsync(userId);

                var newJwtToken = await _userService.Generate(user);

                return Ok(new { token = newJwtToken });
            } catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        [HttpGet("[controller]/[action]")]
        public IActionResult CheckStatus()
        {
            return Ok();
        }

        [HttpPost("[controller]/[action]")]
        public async Task<IActionResult?> GetUserNameFromJwt([FromBody] JwtRequest? jwt)
        {
            var tokenString = "";

            if (string.IsNullOrEmpty(jwt?.Jwt))
            {
                tokenString = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            }
            else
            {
                tokenString = jwt.Jwt;
            }

            var userId = await _userService.GetUserIdFromToken(tokenString);

            if(userId == null) { return null; }

            var user = await _userManager.FindByIdAsync(userId);

            if(user == null) { return null; }

            return Ok(user.UserName);
        }

        [HttpPost("[controller]/[action]")]
        public async Task<IActionResult?> GetUserIdFromJwt([FromBody] JwtRequest? jwt)
        {
            var tokenString = "";

            if (string.IsNullOrEmpty(jwt?.Jwt))
            {
                tokenString = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            }
            else
            {
                tokenString = jwt.Jwt;
            }

            var userId = await _userService.GetUserIdFromToken(tokenString);

            if (userId == null) { return null; }

            return Ok(userId);
        }
    }
}
