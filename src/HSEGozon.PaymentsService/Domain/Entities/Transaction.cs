namespace HSEGozon.PaymentsService.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public Guid OrderId { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum TransactionType
{
    Debit,
    Credit
}

