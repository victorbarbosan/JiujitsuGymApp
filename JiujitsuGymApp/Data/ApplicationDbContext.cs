using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<ClassSchedule> ClassSchedules { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Entity setting
            modelBuilder.Entity<User>(entity =>
            {

                // Table name
                entity.ToTable("Users");

                // Store as "White", "Blue", etc
                entity.Property(u => u.Belt)
                    .HasConversion<string>();

            });

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Prevent duplicate check-ins per student per class
            modelBuilder.Entity<Attendance>()
                .HasIndex(a => new { a.ClassId, a.UserId })
                .IsUnique();
        }
    }
}