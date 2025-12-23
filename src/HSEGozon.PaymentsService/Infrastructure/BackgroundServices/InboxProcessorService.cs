using HSEGozon.PaymentsService.Abstractions;
using HSEGozon.PaymentsService.Domain.Entities;
using HSEGozon.PaymentsService.Domain.Messages;
using HSEGozon.PaymentsService.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HSEGozon.PaymentsService.Infrastructure.BackgroundServices;

public class InboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InboxProcessorService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);

    public InboxProcessorService(
        IServiceProvider serviceProvider,
        ILogger<InboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inbox Processor Service started");

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
                    "payments.process-payment",
                    async (message, messageId) =>
                    {
                        using var handlerScope = _serviceProvider.CreateScope();
                        await ProcessIncomingMessageAsync(message, messageId, handlerScope);
                    });

                _logger.LogInformation("Successfully connected to RabbitMQ and started consuming");
                break;
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
                await Task.Delay(_pollingInterval, stoppingToken);
            }
        }
        finally
        {
            messageConsumer?.StopConsuming();
            scope?.Dispose();
            _logger.LogInformation("Inbox Processor Service stopped");
        }
    }


    private async Task ProcessIncomingMessageAsync(string message, string messageId, IServiceScope scope)
    {
        var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxMessageRepository>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentProcessingService>();

        var existingInboxMessage = await inboxRepository.GetByMessageIdAsync(messageId);

        if (existingInboxMessage != null && existingInboxMessage.Status == InboxMessageStatus.Processed)
        {
            _logger.LogInformation("Message {MessageId} already processed, skipping", messageId);
            return;
        }

        var context = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
        InboxMessage inboxMessage;
        if (existingInboxMessage == null)
        {
            inboxMessage = new InboxMessage
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                MessageType = "ProcessPayment",
                Payload = message,
                Status = InboxMessageStatus.Pending,
                ReceivedAt = DateTime.UtcNow
            };
            await inboxRepository.AddAsync(inboxMessage);
            await context.SaveChangesAsync();
        }
        else
        {
            inboxMessage = existingInboxMessage;
        }

        try
        {
            inboxMessage.Status = InboxMessageStatus.Processing;
            await inboxRepository.UpdateAsync(inboxMessage);
            await context.SaveChangesAsync();

            var paymentMessage = JsonSerializer.Deserialize<ProcessPaymentMessage>(message);
            if (paymentMessage == null)
            {
                throw new InvalidOperationException("Failed to deserialize payment message");
            }

            await paymentService.ProcessPaymentAsync(
                paymentMessage.MessageId,
                paymentMessage.OrderId,
                paymentMessage.UserId,
                paymentMessage.Amount);

            inboxMessage.Status = InboxMessageStatus.Processed;
            inboxMessage.ProcessedAt = DateTime.UtcNow;
            await inboxRepository.UpdateAsync(inboxMessage);
            await context.SaveChangesAsync();

            _logger.LogInformation("Payment message {MessageId} processed successfully", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing inbox message {MessageId}", messageId);
            inboxMessage.Status = InboxMessageStatus.Failed;
            await inboxRepository.UpdateAsync(inboxMessage);
            await context.SaveChangesAsync();
            throw;
        }
    }
}

