using System.Security.Cryptography;
using System.Text;

namespace Marketplace_System.Services
{
    public static class PasswordHasher
    {
        public static string Hash(string rawPassword)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawPassword));
            return Convert.ToHexString(bytes);
        }
    }
}