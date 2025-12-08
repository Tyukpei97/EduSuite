namespace Task3.Core.Models
{
    public sealed class Client
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public decimal TotalSpent { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Comment { get; set; }
    }
}
