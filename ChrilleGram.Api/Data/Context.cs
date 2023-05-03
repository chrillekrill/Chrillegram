using ChrilleGram.Api.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChrilleGram.Api.Data
{
    public class Context : IdentityDbContext
    {
        public Context(DbContextOptions<Context> options)
        : base(options)
        {
        }
        public DbSet<Image> Image { get; set; }
    }
}
