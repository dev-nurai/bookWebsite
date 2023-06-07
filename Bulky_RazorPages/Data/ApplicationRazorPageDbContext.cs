using Bulky_RazorPages.Models;
using Microsoft.EntityFrameworkCore;

namespace Bulky_RazorPages.Data
{
    public class ApplicationRazorPageDbContext : DbContext
    {
        public ApplicationRazorPageDbContext(DbContextOptions<ApplicationRazorPageDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "SciFi", DisplayOrder = 2 },
                new Category { Id = 3, Name = "History", DisplayOrder = 3 }
                );
        }

    }
}
