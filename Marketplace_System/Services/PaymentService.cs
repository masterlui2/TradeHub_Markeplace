using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marketplace_System.Data;
using Marketplace_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Marketplace_System.Services
{
    public sealed class PaymentService
    {
        public async Task<List<Payment>> GetAllAsync()
        {
            await using var db = new AppDbContext();
            return await db.Payments
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

      

        public async Task<bool> UpdateAsync(Payment payment)
        {
            await using var db = new AppDbContext();
            var existing = await db.Payments.FirstOrDefaultAsync(p => p.Id == payment.Id);
            if (existing is null)
                return false;

            existing.ReferenceNumber = payment.ReferenceNumber.Trim();
            existing.OrderNumber = payment.OrderNumber.Trim();
            existing.PayerName = payment.PayerName.Trim();
            existing.RecipientName = payment.RecipientName.Trim();
            existing.Method = payment.Method.Trim();
            existing.Status = payment.Status.Trim();
            existing.Amount = payment.Amount;
            existing.Notes = payment.Notes.Trim();
            existing.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int paymentId)
        {
            await using var db = new AppDbContext();
            var existing = await db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId);
            if (existing is null)
                return false;

            db.Payments.Remove(existing);
            await db.SaveChangesAsync();
            return true;
        }
    }
}