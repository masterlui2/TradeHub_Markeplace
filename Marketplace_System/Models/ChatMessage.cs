using System.ComponentModel.DataAnnotations;

using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace_System.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public int ThreadId { get; set; }

        [Required]
        public int SenderUserId { get; set; }

        [Required]
        public int ReceiverUserId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Body { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}