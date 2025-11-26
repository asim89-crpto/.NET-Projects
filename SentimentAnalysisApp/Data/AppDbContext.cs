using Microsoft.EntityFrameworkCore;
using NewApp.Models;

namespace NewApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Review> Reviews { get; set; }
    }
}
