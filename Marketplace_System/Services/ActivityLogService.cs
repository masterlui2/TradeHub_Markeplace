using Marketplace_System.Data;
using Marketplace_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace_System.Services
{
    public sealed class ActivityLogService
    {
        private const string EnsureTableSql = @"
IF OBJECT_ID('dbo.ActivityLogs', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ActivityLogs](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [UserId] INT NULL,
        [Category] NVARCHAR(64) NOT NULL,
        [Action] NVARCHAR(140) NOT NULL,
        [Details] NVARCHAR(MAX) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL
    );
    CREATE INDEX [IX_ActivityLogs_CreatedAt] ON [dbo].[ActivityLogs]([CreatedAt]);
    CREATE INDEX [IX_ActivityLogs_UserId] ON [dbo].[ActivityLogs]([UserId]);
END";

        public async Task LogAsync(string action, string details, int? userId = null, string category = "General")
        {
            await using var db = new AppDbContext();
            await EnsureTableAsync(db);

            db.ActivityLogs.Add(new ActivityLog
            {
                UserId = userId,
                Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
                Action = action.Trim(),
                Details = details.Trim(),
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        public async Task<List<ActivityLog>> GetRecentAsync(int take, int? userId = null)
        {
            await using var db = new AppDbContext();
            await EnsureTableAsync(db);

            IQueryable<ActivityLog> query = db.ActivityLogs.AsNoTracking();

            if (userId.HasValue)
                query = query.Where(x => x.UserId == userId.Value);

            return await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        private static async Task EnsureTableAsync(AppDbContext db)
        {
            await db.Database.ExecuteSqlRawAsync(EnsureTableSql);
        }
    }
}