using System.Text.RegularExpressions;
using Marketplace_System.Data;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace_System.Services
{
    public class AuthService
    {
        public async Task<(bool Success, string Message)> RegisterAsync(
            string fullName,
            string email,
            string mobileNumber,
            string city,
            string password)
        {
            await using AppDbContext dbContext = new();
            if (!IsStrongPassword(password))
            {
                return (false, "Password must be at least 8 characters and include uppercase, lowercase, number, and special character.");
            }
            string normalizedEmail = email.Trim().ToLowerInvariant();
            bool emailExists = await dbContext.Users.AnyAsync(u => u.Email == normalizedEmail);
            if (emailExists)
            {
                return (false, "This email is already registered.");
            }

            User user = new()
            {
                FullName = fullName.Trim(),
                Email = normalizedEmail,
                MobileNumber = mobileNumber.Trim(),
                City = city.Trim(),
                PasswordHash = PasswordHasher.Hash(password)
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return (true, "Registration successful!");
        }

        public async Task<User?> LoginAsync(string usernameOrEmail, string password)
        {
            await using AppDbContext dbContext = new();
            string lookup = usernameOrEmail.Trim().ToLowerInvariant();
            string hashedPassword = PasswordHasher.Hash(password);

            return await dbContext.Users.FirstOrDefaultAsync(u =>
   ((u.Email != null && u.Email.ToLower() == lookup) || (u.FullName != null && u.FullName.ToLower() == lookup)) &&
                u.PasswordHash == hashedPassword);
        }
        private static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                return false;
            }

            bool hasUppercase = Regex.IsMatch(password, "[A-Z]");
            bool hasLowercase = Regex.IsMatch(password, "[a-z]");
            bool hasDigit = Regex.IsMatch(password, "\\d");
            bool hasSpecialCharacter = Regex.IsMatch(password, "[^a-zA-Z0-9]");

            return hasUppercase && hasLowercase && hasDigit && hasSpecialCharacter;
        }
    }
}