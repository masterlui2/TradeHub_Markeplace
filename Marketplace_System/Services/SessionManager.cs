namespace Marketplace_System.Services
{
    public static class SessionManager
    {
        public static int CurrentUserId { get; private set; }
        public static string CurrentUserFullName { get; private set; } = "Guest";

        public static void SetCurrentUser(int userId, string fullName)
        {
            CurrentUserId = userId;
            CurrentUserFullName = string.IsNullOrWhiteSpace(fullName) ? "Guest" : fullName;
        }

        public static void Clear()
        {
            CurrentUserId = 0;
            CurrentUserFullName = "Guest";
        }
    }
}