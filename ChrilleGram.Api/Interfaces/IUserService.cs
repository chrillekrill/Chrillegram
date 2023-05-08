using Microsoft.AspNetCore.Identity;

namespace ChrilleGram.Api.Interfaces
{
    public interface IUserService
    {
        Task<IdentityUser?> AuthenticateUserAsync(string email, string password);
    }
}
