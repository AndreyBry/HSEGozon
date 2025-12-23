using HSEGozon.OrdersService.Abstractions;
using HSEGozon.OrdersService.Domain.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HSEGozon.OrdersService.Infrastructure.BackgroundServices;

public class PaymentStatusConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentStatusConsumerService> _logger;

    public PaymentStatusConsumerService(
        IServiceProvider serviceProvider,
        ILogger<PaymentStatusConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment Status Consumer Service started");

        const int maxRetries = 10;
        const int retryDelaySeconds = 5;
        int retryCount = 0;
        IServiceScope? scope = null;
        IMessageConsumer? messageConsumer = null;

        while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                scope = _serviceProvider.CreateScope();
                messageConsumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();

                await messageConsumer.StartConsumingAsync(
                    "orders.payment.status",
                    async (message, messageId) =>
                    {
                        using var handlerScope = _serviceProvider.CreateScope();
                        var orderService = handlerScope.ServiceProvider.GetRequiredService<IOrderService>();
                        await ProcessPaymentStatusAsync(message, messageId, orderService);
                    });

                _logger.LogInformation("Successfully connected to RabbitMQ and started consuming");
                break; // Success, exit retry loop
            }
            catch (Exception ex)
            {
                scope?.Dispose();
                scope = null;
                messageConsumer = null;
                
                retryCount++;
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ (attempt {RetryCount}/{MaxRetries}). Retrying in {Delay}s...", 
                    retryCount, maxRetries, retryDelaySeconds);
                
                if (retryCount < maxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), stoppingToken);
                }
                else
                {
                    _logger.LogError("Failed to connect to RabbitMQ after {MaxRetries} attempts. Service will continue retrying...", maxRetries);
                    await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds * 2), stoppingToken);
                }
            }
        }

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
        finally
        {
            messageConsumer?.StopConsuming();
            scope?.Dispose();
            _logger.LogInformation("Payment Status Consumer Service stopped");
        }
    }

    private async Task ProcessPaymentStatusAsync(string message, string messageId, IOrderService orderService)
    {
        try
        {
            var statusMessage = JsonSerializer.Deserialize<PaymentStatusMessage>(message);
            if (statusMessage == null)
            {
                _logger.LogWarning("Failed to deserialize payment status message: {MessageId}", messageId);
                return;
            }

            await orderService.UpdateOrderStatusAsync(
                statusMessage.OrderId,
                statusMessage.Status);

            _logger.LogInformation("Order {OrderId} status updated to {Status}",
                statusMessage.OrderId, statusMessage.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment status message: {MessageId}", messageId);
            throw;
        }
    }
}

