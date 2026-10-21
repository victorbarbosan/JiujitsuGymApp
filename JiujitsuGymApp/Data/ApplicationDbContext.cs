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
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

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

            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.Author)
                .WithMany()
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.Parent)
                .WithMany(a => a.Replies)
                .HasForeignKey(a => a.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.Property(s => s.Status)
                    .HasConversion<string>();

                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.SubscriptionPlan)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(s => s.SubscriptionPlanId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(s => s.StripeSubscriptionId)
                    .IsUnique();
            });

            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.Property(p => p.Interval)
                    .HasConversion<string>();
            });
        }
    }
}