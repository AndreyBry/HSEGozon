using HSEGozon.OrdersService.Domain.DTOs;

namespace HSEGozon.OrdersService.Abstractions;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<IEnumerable<OrderResponse>> GetOrdersAsync(Guid userId);
    Task<OrderResponse?> GetOrderAsync(Guid orderId);
    Task UpdateOrderStatusAsync(Guid orderId, string status);
}

