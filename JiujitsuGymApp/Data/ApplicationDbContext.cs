using Microsoft.EntityFrameworkCore;
using JiujitsuGymApp.Models;

namespace JiujitsuGymApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed initial BJJ products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "BJJ Gi White", Price = 129.99m, Description = "Premium Jiu-Jitsu Gi", Category = "Gis", CreatedDate = new DateTime(2025, 01, 01 ,0 ,0 ,0, DateTimeKind.Utc) },
                new Product { Id = 2, Name = "Rash Guard", Price = 49.99m, Description = "Compression rash guard", Category = "Apparel", CreatedDate = new DateTime(2025, 01, 01 ,0 ,0 ,0, DateTimeKind.Utc) },
                new Product { Id = 3, Name = "BJJ Belt White", Price = 24.99m, Description = "White belt for beginners", Category = "Belts", CreatedDate = new DateTime(2025, 01, 01 ,0 ,0 ,0, DateTimeKind.Utc) },
                new Product { Id = 4, Name = "Finger Tape", Price = 9.99m, Description = "Protective tape for fingers", Category = "Accessories", CreatedDate = new DateTime(2025, 01, 01 ,0 ,0 ,0, DateTimeKind.Utc) },
                new Product { Id = 5, Name = "Mouth Guard", Price = 19.99m, Description = "Protective mouth guard", Category = "Safety", CreatedDate = new DateTime(2025, 01, 01 ,0 ,0 ,0, DateTimeKind.Utc) }
            );
        }
    }
}   