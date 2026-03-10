
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Marketplace_System.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Marketplace_System.Services
{
    public sealed class SuperAdminService
    {
        public async Task<SuperAdminDashboardData> GetDashboardDataAsync()
        {
            await using var db = new AppDbContext();
            var users = await db.Users.AsNoTracking().ToListAsync();
            var orders = await db.Orders.AsNoTracking().ToListAsync();
            var listings = await db.ProductListings.AsNoTracking().ToListAsync();
            var payments = await db.Payments.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToListAsync();

            var salesCount = orders.Count(o => o.Status == Order.StatusCompleted || o.Status == Order.StatusPaid);
            var revenue = payments.Where(p => p.Status == Payment.StatusPaid).Sum(p => p.Amount);

            var trend = orders
                .Where(o => o.CreatedAt >= DateTime.UtcNow.AddDays(-7))
                .GroupBy(o => o.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new SalesTrendPoint
                {
                    Label = g.Key.ToString("MMM dd"),
                    Revenue = g.Sum(x => x.UnitPrice * x.QuantityKilos)
                })
                .ToList();

            var recentTransactions = payments.Take(8).Select(p => new RecentTransactionRow
            {
                ReferenceNumber = p.ReferenceNumber,
                OrderNumber = p.OrderNumber,
                PayerName = p.PayerName,
                Amount = p.Amount,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            }).ToList();

            var alerts = new List<string>();
            var pendingPayments = payments.Count(p => p.Status == Payment.StatusPending);
            if (pendingPayments > 0)
                alerts.Add($"{pendingPayments} pending payments need review.");

            var failedPayments = payments.Count(p => p.Status == Payment.StatusFailed);
            if (failedPayments > 0)
                alerts.Add($"{failedPayments} failed payments detected.");

            var lowStock = listings.Count(l => l.AvailableKilos < 10);
            if (lowStock > 0)
                alerts.Add($"{lowStock} listings are low stock (<10kg).");

            return new SuperAdminDashboardData
            {
                TotalUsers = users.Count,
                TotalSales = salesCount,
                Revenue = revenue,
                ActiveListings = listings.Count,
                SalesTrends = new(trend),
                RecentTransactions = new(recentTransactions),
                Alerts = new(alerts)
            };
        }

        public async Task<List<SuperAdminUserRow>> GetUsersAsync()
        {
            await using var db = new AppDbContext();
            return await db.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt)
                .Select(u => new SuperAdminUserRow
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    MobileNumber = u.MobileNumber,
                    City = u.City,
                    IsSuspended = u.IsSuspended,
                    CreatedAt = u.CreatedAt
                }).ToListAsync();
        }

        public async Task CreateUserAsync(User user)
        {
            await using var db = new AppDbContext();
            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(SuperAdminUserRow row)
        {
            await using var db = new AppDbContext();
            var user = await db.Users.FirstAsync(u => u.Id == row.Id);
            user.FullName = row.FullName;
            user.Email = row.Email;
            user.MobileNumber = row.MobileNumber;
            user.City = row.City;
            await db.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int userId)
        {
            await using var db = new AppDbContext();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is null)
                return;

            db.Users.Remove(user);
            await db.SaveChangesAsync();
        }

        public async Task ToggleSuspendAsync(int userId, bool isSuspended)
        {
            await using var db = new AppDbContext();
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            user.IsSuspended = isSuspended;
            await db.SaveChangesAsync();
        }

        public async Task<List<SuperAdminPaymentRow>> GetPaymentsAsync()
        {
            await using var db = new AppDbContext();
            return await db.Payments.AsNoTracking().OrderByDescending(p => p.CreatedAt)
                .Select(p => new SuperAdminPaymentRow
                {
                    Id = p.Id,
                    ReferenceNumber = p.ReferenceNumber,
                    OrderNumber = p.OrderNumber,
                    PayerName = p.PayerName,
                    RecipientName = p.RecipientName,
                    Amount = p.Amount,
                    Method = p.Method,
                    Status = p.Status,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt
                }).ToListAsync();
        }
    }
}