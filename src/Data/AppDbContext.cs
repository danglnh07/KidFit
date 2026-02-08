using KidFit.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KidFit.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<CardCategory> CardCategories { get; set; } = null!;
        public DbSet<Card> Cards { get; set; } = null!;
        public DbSet<Module> Modules { get; set; } = null!;
        public DbSet<Lesson> Lessons { get; set; } = null!;
        public DbSet<ApplicationUser> Accounts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Global query filter 
            modelBuilder.Entity<ApplicationUser>().HasQueryFilter(item => item.IsActive);
            modelBuilder.Entity<CardCategory>().HasQueryFilter(item => !item.IsDeleted);
            modelBuilder.Entity<Card>().HasQueryFilter(item => !item.IsDeleted);
            modelBuilder.Entity<Module>().HasQueryFilter(item => !item.IsDeleted);
            modelBuilder.Entity<Lesson>().HasQueryFilter(item => !item.IsDeleted);

            // Card category entity setup
            modelBuilder.Entity<CardCategory>()
                .HasKey(item => item.Id);
            modelBuilder.Entity<CardCategory>()
                .HasIndex(item => item.Name)
                .IsUnique();

            // Card entity setup
            modelBuilder.Entity<Card>()
                .HasKey(item => item.Id);
            modelBuilder.Entity<Card>()
                .HasOne(item => item.Category)
                .WithMany()
                .HasForeignKey(item => item.CategoryId);
            modelBuilder.Entity<Card>()
                .HasIndex(item => item.Name)
                .IsUnique();

            // Module entity setup
            modelBuilder.Entity<Module>()
                .HasKey(item => item.Id);
            modelBuilder.Entity<Module>()
                .HasIndex(item => item.Name)
                .IsUnique();

            // Lesson entity setup
            modelBuilder.Entity<Lesson>()
                .HasKey(item => item.Id);
            modelBuilder.Entity<Lesson>()
                .HasOne(item => item.Module)
                .WithMany()
                .HasForeignKey(item => item.ModuleId);
            modelBuilder.Entity<Lesson>()
                .HasMany(l => l.Cards)
                .WithMany();

            base.OnModelCreating(modelBuilder);
        }
    }
}
