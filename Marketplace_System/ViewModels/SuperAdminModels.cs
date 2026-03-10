using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Marketplace_System.ViewModels
{
    public sealed class DashboardMetric
    {
        public string Label { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public Brush AccentBrush { get; init; } = Brushes.SteelBlue;
    }

    public sealed class SalesTrendPoint
    {
        public string Label { get; init; } = string.Empty;
        public decimal Revenue { get; init; }
        public double Height { get; init; }
    }

    public sealed class SuperAdminDashboardData
    {
        public int TotalUsers { get; init; }
        public int TotalSales { get; init; }
        public decimal Revenue { get; init; }
        public int ActiveListings { get; init; }
        public ObservableCollection<SalesTrendPoint> SalesTrends { get; set; } = new();
        public ObservableCollection<RecentTransactionRow> RecentTransactions { get; set; } = new();
        public ObservableCollection<string> Alerts { get; set; } = new();
    }

    public sealed class SuperAdminUserRow
    {
        public int Id { get; init; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsSuspended { get; set; }
        public DateTime CreatedAt { get; init; }
        public string Status => IsSuspended ? "Suspended" : "Active";
    }

    public sealed class SuperAdminPaymentRow
    {
        public int Id { get; init; }
        public string ReferenceNumber { get; init; } = string.Empty;
        public string OrderNumber { get; init; } = string.Empty;
        public string PayerName { get; init; } = string.Empty;
        public string RecipientName { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Method { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string Notes { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }

    public sealed class RecentTransactionRow
    {
        public string ReferenceNumber { get; init; } = string.Empty;
        public string OrderNumber { get; init; } = string.Empty;
        public string PayerName { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}