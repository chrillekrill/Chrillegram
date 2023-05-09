using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChrilleGram.Api.Data
{
    public class DataInitializer
    {
        private readonly Context _context;

        public DataInitializer(Context context)
        {
            _context = context;
        }

        public async Task SeedData()
        {
            await _context.Database.MigrateAsync();
            await SeedRoles();
        }

        private async Task CreateRoleIfNotExists(string rolename)
        {
            if (_context.Roles.Any(e => e.Name == rolename))
                return;
            await _context.Roles.AddAsync(new IdentityRole { Name = rolename, NormalizedName = rolename });
            await _context.SaveChangesAsync();
        }
        private async Task SeedRoles()
        {
            await CreateRoleIfNotExists("Admin");
            await CreateRoleIfNotExists("User");
        }
    }
}
