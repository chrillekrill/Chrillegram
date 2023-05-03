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

        public void SeedData()
        {
            _context.Database.Migrate();
        }
    }
}
