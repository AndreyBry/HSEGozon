using System.Text.Json.Serialization;

namespace HSEGozon.OrdersService.Domain.Messages;

public class ProcessPaymentMessage
{
    [JsonPropertyName("orderId")]
    public Guid OrderId { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;
}

