using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace_System.Models
{
    public class ProductListing
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string Category { get; set; } = string.Empty;

        [Range(0.01, 999999.99)]
        public decimal PricePerKilo { get; set; }

        [Range(1, int.MaxValue)]
        public int AvailableKilos { get; set; }

        [Required]
        [MaxLength(250)]
        public string PickupAddress { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ImagePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}