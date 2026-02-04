using System;
using System.Collections.Generic;
using System.Text;

namespace Marketplace_System.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; }
        public string Role { get; set; } // "Admin", "Customer", "Seller"
        public DateTime CreatedAt { get; set; }
    }
}
