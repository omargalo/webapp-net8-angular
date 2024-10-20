using Microsoft.EntityFrameworkCore;
using AppBackend.Models;

namespace AppBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for your tables
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }  // Add UserRoles DbSet
        public DbSet<CatRole> CatRoles { get; set; }    // Add CatRoles DbSet

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: Additional configurations
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId }); // Composite Key for UserRole
        }
    }
}
