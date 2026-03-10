using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace_System.Models
{
    public class Payment
    {
        public const string StatusPending = "Pending";
        public const string StatusPaid = "Paid";
        public const string StatusFailed = "Failed";
        public const string StatusRefunded = "Refunded";

        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string ReferenceNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(24)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string PayerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(24)]
        public string Method { get; set; } = "Cash";

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = StatusPending;

        [Range(0.01, 999999.99)]
        public decimal Amount { get; set; }

        [MaxLength(250)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}