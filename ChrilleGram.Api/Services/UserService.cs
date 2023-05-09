using ChrilleGram.Api.Data;
using ChrilleGram.Api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChrilleGram.Api.Services
{
    public class UserService : IUserService
    {
        private readonly Context _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;
        public UserService(UserManager<IdentityUser> userManager,
        Context context,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
            _configuration = configuration;
        }
        public async Task<IdentityUser?> AuthenticateUserAsync(string email, string password)
        {
            var currentUser = await _userManager.FindByEmailAsync(email);

            if(currentUser == null)
            {
                return null;
            }
            var locked = await _userManager.IsLockedOutAsync(currentUser);
            if (locked)
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

        public string Generate(IdentityUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var userRole = _userManager.GetRolesAsync(user).Result;


            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserName),
            new Claim(ClaimTypes.Role, userRole.First()),
            new Claim("id", user.Id.ToString())
            };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
