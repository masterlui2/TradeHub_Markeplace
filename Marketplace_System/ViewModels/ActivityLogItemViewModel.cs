namespace Marketplace_System.ViewModels
{
    public sealed class ActivityLogItemViewModel
    {
        public string TimeLabel { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string Action { get; init; } = string.Empty;
        public string Details { get; init; } = string.Empty;
    }
}