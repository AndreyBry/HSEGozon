namespace HSEGozon.PaymentsService.Abstractions;

public interface IMessageConsumer
{
    Task StartConsumingAsync(string queueName, Func<string, string, Task> onMessageReceived);
    void StopConsuming();
}

