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
                (u.Email == lookup || u.FullName.ToLower() == lookup) &&
                u.PasswordHash == hashedPassword);
        }
    }
}