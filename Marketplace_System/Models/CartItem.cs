
using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace_System.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string ProductName { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int QuantityKilos { get; set; }

        [Range(0.01, 999999.99)]
        public decimal UnitPrice { get; set; }

        [Required]
        public int BuyerUserId { get; set; }

        [Required]
        public int SellerUserId { get; set; }

        [Required]
        public int ProductListingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}