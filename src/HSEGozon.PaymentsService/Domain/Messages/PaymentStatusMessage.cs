using System.Text.Json.Serialization;

namespace HSEGozon.PaymentsService.Domain.Messages;

public class PaymentStatusMessage
{
    [JsonPropertyName("orderId")]
    public Guid OrderId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;
}

