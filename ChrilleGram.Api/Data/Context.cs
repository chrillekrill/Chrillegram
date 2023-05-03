using ChrilleGram.Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ChrilleGram.Api.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
        : base(options)
        {
        }
        public DbSet<Image> Image { get; set; }
    }
}
