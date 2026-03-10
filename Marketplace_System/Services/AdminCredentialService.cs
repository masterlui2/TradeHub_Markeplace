using System;
using System.IO;
using System.Text.Json;

namespace Marketplace_System.Services
{
    public enum AdminRole
    {
        None,
        Admin,
        SuperAdmin
    }

    public sealed class AdminCredentialService
    {
        private const string CredentialsFileName = "admin-credentials.json";

        public AdminRole Validate(string usernameOrEmail, string password)
        {
            var credentials = LoadOrCreateCredentials();
            var normalizedInput = (usernameOrEmail ?? string.Empty).Trim();
            var passwordHash = PasswordHasher.Hash(password ?? string.Empty);

            if (string.Equals(normalizedInput, credentials.SuperAdmin.Username, StringComparison.OrdinalIgnoreCase)
                && passwordHash == credentials.SuperAdmin.PasswordHash)
            {
                return AdminRole.SuperAdmin;
            }

            if (string.Equals(normalizedInput, credentials.Admin.Username, StringComparison.OrdinalIgnoreCase)
                && passwordHash == credentials.Admin.PasswordHash)
            {
                return AdminRole.Admin;
            }

            return AdminRole.None;
        }

        public string CredentialsPath => GetCredentialsFilePath();

        private static AdminCredentialsModel LoadOrCreateCredentials()
        {
            var filePath = GetCredentialsFilePath();
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(filePath))
            {
                var defaults = AdminCredentialsModel.CreateDefaults();
                var newJson = JsonSerializer.Serialize(defaults, JsonOptions());
                File.WriteAllText(filePath, newJson);
                return defaults;
            }

            try
            {
                var json = File.ReadAllText(filePath);
                var model = JsonSerializer.Deserialize<AdminCredentialsModel>(json, JsonOptions());
                if (model?.Admin?.Username is null || model.SuperAdmin?.Username is null)
                {
                    throw new InvalidDataException("Invalid credentials format.");
                }

                return model;
            }
            catch
            {
                var defaults = AdminCredentialsModel.CreateDefaults();
                var fallbackJson = JsonSerializer.Serialize(defaults, JsonOptions());
                File.WriteAllText(filePath, fallbackJson);
                return defaults;
            }
        }

        private static JsonSerializerOptions JsonOptions() => new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        private static string GetCredentialsFilePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "FarmHub", CredentialsFileName);
        }

        private sealed class AdminCredentialsModel
        {
            public CredentialEntry Admin { get; set; } = new();
            public CredentialEntry SuperAdmin { get; set; } = new();

            public static AdminCredentialsModel CreateDefaults() => new()
            {
                Admin = new CredentialEntry
                {
                    Username = "admin",
                    PasswordHash = PasswordHasher.Hash("Admin@123")
                },
                SuperAdmin = new CredentialEntry
                {
                    Username = "superadmin",
                    PasswordHash = PasswordHasher.Hash("SuperAdmin@123")
                }
            };
        }

        private sealed class CredentialEntry
        {
            public string Username { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
        }
    }
}