using HSEGozon.OrdersService.Abstractions;
using HSEGozon.OrdersService.Domain.Entities;
using HSEGozon.OrdersService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HSEGozon.OrdersService.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        return order;
    }

    public async Task<Order?> GetByIdAsync(Guid orderId)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
    }
}

