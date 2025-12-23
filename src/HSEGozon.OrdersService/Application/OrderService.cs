using HSEGozon.OrdersService.Abstractions;
using HSEGozon.OrdersService.Domain.Entities;
using HSEGozon.OrdersService.Domain.DTOs;
using HSEGozon.OrdersService.Domain.Messages;
using HSEGozon.OrdersService.Infrastructure.Data;
using System.Text.Json;

namespace HSEGozon.OrdersService.Application;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOutboxMessageRepository _outboxMessageRepository;
    private readonly OrdersDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IOutboxMessageRepository outboxMessageRepository,
        OrdersDbContext context,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _outboxMessageRepository = outboxMessageRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Amount = request.Amount,
                Description = request.Description,
                Status = OrderStatus.NEW,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var messageId = Guid.NewGuid().ToString();
            var paymentMessage = new ProcessPaymentMessage
            {
                OrderId = order.Id,
                UserId = request.UserId,
                Amount = request.Amount,
                MessageId = messageId
            };

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                MessageType = "ProcessPayment",
                Payload = JsonSerializer.Serialize(paymentMessage),
                Status = OutboxMessageStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                RetryCount = 0
            };

            await _orderRepository.AddAsync(order);
            await _outboxMessageRepository.AddAsync(outboxMessage);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Order {OrderId} created and payment message queued", order.Id);

            return MapToResponse(order);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating order");
            throw;
        }
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersAsync(Guid userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return orders.Select(MapToResponse);
    }

    public async Task<OrderResponse?> GetOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            return null;
        }

        return MapToResponse(order);
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, string status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found for status update", orderId);
            return;
        }

        var newStatus = status.ToUpper() switch
        {
            "SUCCESS" => OrderStatus.FINISHED,
            "FAILED" => OrderStatus.CANCELLED,
            _ => order.Status
        };

        if (order.Status != newStatus)
        {
            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, newStatus);
        }
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            Amount = order.Amount,
            Description = order.Description,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }
}

