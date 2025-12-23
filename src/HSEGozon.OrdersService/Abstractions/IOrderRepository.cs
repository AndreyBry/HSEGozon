using HSEGozon.OrdersService.Domain.Entities;

namespace HSEGozon.OrdersService.Abstractions;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order);
    Task<Order?> GetByIdAsync(Guid orderId);
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
    Task UpdateAsync(Order order);
}

