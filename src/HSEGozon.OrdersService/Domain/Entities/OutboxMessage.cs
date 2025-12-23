namespace HSEGozon.OrdersService.Domain.Entities;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public OutboxMessageStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int RetryCount { get; set; }
}

public enum OutboxMessageStatus
{
    Pending,
    Published,
    Failed
}

