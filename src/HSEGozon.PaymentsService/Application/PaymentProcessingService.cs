using HSEGozon.PaymentsService.Abstractions;
using HSEGozon.PaymentsService.Domain.Entities;
using HSEGozon.PaymentsService.Domain.Messages;
using HSEGozon.PaymentsService.Infrastructure.Data;

namespace HSEGozon.PaymentsService.Application;

public class PaymentProcessingService : IPaymentProcessingService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly PaymentsDbContext _context;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<PaymentProcessingService> _logger;

    public PaymentProcessingService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IOutboxMessageRepository outboxMessageRepository,
        PaymentsDbContext context,
        IMessagePublisher messagePublisher,
        ILogger<PaymentProcessingService> logger)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _context = context;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task ProcessPaymentAsync(string messageId, Guid orderId, Guid userId, decimal amount)
    {
        var alreadyProcessed = await _transactionRepository.GetByMessageIdAndOrderIdAsync(messageId, orderId) != null;

        if (alreadyProcessed)
        {
            _logger.LogInformation("Payment already processed for order {OrderId} with message {MessageId}", orderId, messageId);
            await PublishPaymentStatusAsync(orderId, "SUCCESS", null, messageId);
            return;
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var account = await _accountRepository.GetByUserIdWithLockAsync(userId);

            if (account == null)
            {
                _logger.LogWarning("Account not found for user {UserId}, order {OrderId}", userId, orderId);
                await transaction.RollbackAsync();
                await PublishPaymentStatusAsync(orderId, "FAILED", "Account not found", messageId);
                return;
            }

            if (account.Balance < amount)
            {
                _logger.LogWarning("Insufficient balance for user {UserId}, order {OrderId}. Balance: {Balance}, Required: {Amount}",
                    userId, orderId, account.Balance, amount);
                await transaction.RollbackAsync();
                await PublishPaymentStatusAsync(orderId, "FAILED", "Insufficient balance", messageId);
                return;
            }

            account.Balance -= amount;
            account.UpdatedAt = DateTime.UtcNow;

            var transactionRecord = new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                OrderId = orderId,
                MessageId = messageId,
                Amount = amount,
                Type = TransactionType.Debit,
                CreatedAt = DateTime.UtcNow
            };

            var statusMessage = new PaymentStatusMessage
            {
                OrderId = orderId,
                Status = "SUCCESS",
                MessageId = Guid.NewGuid().ToString()
            };

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = "PaymentStatus",
                Payload = System.Text.Json.JsonSerializer.Serialize(statusMessage),
                Status = OutboxMessageStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            };

            await _accountRepository.UpdateAsync(account);
            await _transactionRepository.AddAsync(transactionRecord);
            await _outboxMessageRepository.AddAsync(outboxMessage);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Payment processed successfully for order {OrderId}, user {UserId}, amount {Amount}",
                orderId, userId, amount);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error processing payment for order {OrderId}", orderId);
            await PublishPaymentStatusAsync(orderId, "FAILED", "Internal error", messageId);
            throw;
        }
    }

    private async Task PublishPaymentStatusAsync(Guid orderId, string status, string? reason, string originalMessageId)
    {
        var statusMessage = new PaymentStatusMessage
        {
            OrderId = orderId,
            Status = status,
            Reason = reason,
            MessageId = Guid.NewGuid().ToString()
        };

        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            MessageType = "PaymentStatus",
            Payload = System.Text.Json.JsonSerializer.Serialize(statusMessage),
            Status = OutboxMessageStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            RetryCount = 0
        };

        await _outboxMessageRepository.AddAsync(outboxMessage);
        await _context.SaveChangesAsync();
    }
}

