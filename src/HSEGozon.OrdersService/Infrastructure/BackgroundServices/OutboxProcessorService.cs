using HSEGozon.OrdersService.Abstractions;
using HSEGozon.OrdersService.Domain.Entities;
using HSEGozon.OrdersService.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HSEGozon.OrdersService.Infrastructure.BackgroundServices;

public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(2);

    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in outbox processor");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Processor Service stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

        var pendingMessages = await outboxRepository.GetPendingMessagesAsync(10);

        foreach (var message in pendingMessages)
        {
            try
            {
                await messagePublisher.PublishAsync(
                    exchange: "payments",
                    routingKey: "process.payment",
                    messageType: message.MessageType,
                    payload: message.Payload);

                message.Status = OutboxMessageStatus.Published;
                message.PublishedAt = DateTime.UtcNow;
                await outboxRepository.UpdateAsync(message);
                var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
                await context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Outbox message {MessageId} published successfully", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing outbox message {MessageId}", message.Id);
                message.RetryCount++;

                if (message.RetryCount >= 5)
                {
                    message.Status = OutboxMessageStatus.Failed;
                }
                await outboxRepository.UpdateAsync(message);
                var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

