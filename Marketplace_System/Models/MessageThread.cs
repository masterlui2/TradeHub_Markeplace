using System;
using System.ComponentModel.DataAnnotations;

namespace Marketplace_System.Models
{
    public class MessageThread
    {
        public int Id { get; set; }

        [Required]
        public int UserOneId { get; set; }

        [Required]
        public int UserTwoId { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}