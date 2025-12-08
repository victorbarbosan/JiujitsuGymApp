using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using JiujitsuGymApp.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace JiujitsuGymApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
	{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
		public DbSet<Class> Classes { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
			base.OnModelCreating(modelBuilder);

			// Seed initial BJJ products
			modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "BJJ Gi White", Price = 129.99m, Description = "Premium Jiu-Jitsu Gi", Category = "Gis", CreatedDate = new DateTime(2025, 01, 01 ,0 ,0 ,0, DateTimeKind.Utc) },
                new Product { Id = 2, Name = "Rash Guard", Price = 49.99m, Description = "Compression rash guard", Category = "Apparel", CreatedDate = new DateTime(2025, 01, 01 ,0 ,0 ,0, DateTimeKind.Utc) },
                new Product { Id = 3, Name = "BJJ Belt White", Price = 24.99m, Description = "White belt for beginners", Category = "Belts", CreatedDate = new DateTime(2025, 01, 01 ,0 ,0 ,0, DateTimeKind.Utc) },
                new Product { Id = 4, Name = "Finger Tape", Price = 9.99m, Description = "Protective tape for fingers", Category = "Accessories", CreatedDate = new DateTime(2025, 01, 01 ,0 ,0 ,0, DateTimeKind.Utc) },
                new Product { Id = 5, Name = "Mouth Guard", Price = 19.99m, Description = "Protective mouth guard", Category = "Safety", CreatedDate = new DateTime(2025, 01, 01 ,0 ,0 ,0, DateTimeKind.Utc) }
            );


            // User Entity setting
            modelBuilder.Entity<User>(entity =>
            {

                // Table name
                entity.ToTable("Users");

				// Store as "White", "Blue", etc
				entity.Property(u => u.Belt)
                    .HasConversion<string>(); 

            });
        }
    }
}   