
using Marketplace_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace_System.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<ProductListing> ProductListings => Set<ProductListing>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=LAPTOP-IKO6DT7P\\SQLEXPRESS;Database=FarmHubDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<ProductListing>()
               .Property(p => p.PricePerKilo)
               .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProductListing>()
                .HasIndex(p => p.CreatedAt);
        }
    }
}