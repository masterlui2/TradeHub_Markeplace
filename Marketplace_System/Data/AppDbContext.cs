using Marketplace_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace_System.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<ProductListing> ProductListings => Set<ProductListing>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<MessageThread> MessageThreads => Set<MessageThread>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

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

            modelBuilder.Entity<ProductListing>()
                .HasIndex(p => p.SellerUserId);

            modelBuilder.Entity<CartItem>()
                .Property(c => c.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CartItem>()
                .HasIndex(c => c.BuyerUserId);

            modelBuilder.Entity<CartItem>()
                .HasIndex(c => c.SellerUserId);

            modelBuilder.Entity<CartItem>()
                .HasIndex(c => c.ProductListingId);

            modelBuilder.Entity<Order>()
                .Property(o => o.UnitPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.BuyerUserId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.SellerUserId);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Status);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UpdatedAt);
            modelBuilder.Entity<Payment>()
               .Property(p => p.Amount)
               .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.ReferenceNumber)
                .IsUnique();

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.OrderNumber);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.Status);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.CreatedAt);

            modelBuilder.Entity<MessageThread>()
                .HasIndex(t => new { t.UserOneId, t.UserTwoId })
                .IsUnique();

            modelBuilder.Entity<MessageThread>()
                .HasIndex(t => t.UpdatedAt);

            modelBuilder.Entity<ChatMessage>()
                .HasIndex(m => m.ThreadId);

            modelBuilder.Entity<ChatMessage>()
                .HasIndex(m => m.CreatedAt);
            modelBuilder.Entity<ActivityLog>()
            .HasIndex(a => a.CreatedAt);

            modelBuilder.Entity<ActivityLog>()
                .HasIndex(a => a.UserId);

            modelBuilder.Entity<ActivityLog>()
                .Property(a => a.Category)
                .HasMaxLength(64);

            modelBuilder.Entity<ActivityLog>()
                .Property(a => a.Action)
                .HasMaxLength(140);

        }
    }
}