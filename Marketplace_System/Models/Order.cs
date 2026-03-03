using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace_System.Models
{
    public class Order
    {
        public const string StatusPendingPayment = "Pending payment";
        public const string StatusPaid = "Paid";
        public const string StatusPreparing = "Preparing";
        public const string StatusReadyForPickup = "Ready for pickup";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";

        public int Id { get; set; }

        [Required]
        [MaxLength(24)]
        public string OrderNumber { get; set; } = string.Empty;

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

        [Required]
        [MaxLength(32)]
        public string Status { get; set; } = StatusPendingPayment;

        [MaxLength(300)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }
        public DateTime? PreparingAt { get; set; }
        public DateTime? ReadyForPickupAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}