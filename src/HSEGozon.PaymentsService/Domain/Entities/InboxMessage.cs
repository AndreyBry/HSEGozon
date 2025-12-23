namespace HSEGozon.PaymentsService.Domain.Entities;

public class InboxMessage
{
    public Guid Id { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public InboxMessageStatus Status { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public enum InboxMessageStatus
{
    Pending,
    Processing,
    Processed,
    Failed
}

