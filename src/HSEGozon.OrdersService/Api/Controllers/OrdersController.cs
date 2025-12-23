using HSEGozon.OrdersService.Abstractions;
using HSEGozon.OrdersService.Api.Examples;
using HSEGozon.OrdersService.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace HSEGozon.OrdersService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpPost]
    [SwaggerRequestExample(typeof(CreateOrderRequest), typeof(CreateOrderRequestExample))]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest? request)
    {
        if (request == null)
        {
            return BadRequest(new { 
                error = "Request body is required. Expected JSON object with userId, amount, and description",
                example = new { 
                    userId = "123e4567-e89b-12d3-a456-426614174000",
                    amount = 500.00,
                    description = "Ноутбук ASUS ROG Strix"
                }
            });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();
            
            return BadRequest(new { 
                error = "Validation failed", 
                details = errors,
                example = new { 
                    userId = "123e4567-e89b-12d3-a456-426614174000",
                    amount = 500.00,
                    description = "Ноутбук ASUS ROG Strix"
                }
            });
        }

        try
        {
            _logger.LogInformation("Creating order for user {UserId}, amount: {Amount}, description: {Description}", 
                request.UserId, request.Amount, request.Description);
            var order = await _orderService.CreateOrderAsync(request);
            _logger.LogInformation("Order created successfully, order ID: {OrderId}, status: {Status}", 
                order.Id, order.Status);
            return CreatedAtAction(nameof(GetOrder), new { orderId = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for user {UserId}", request?.UserId);
            return BadRequest(new { error = "Failed to create order", message = ex.Message });
        }
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders([FromRoute] Guid userId)
    {
        _logger.LogInformation("Getting orders for user {UserId}", userId);
        var orders = await _orderService.GetOrdersAsync(userId);
        _logger.LogInformation("Retrieved {Count} orders for user {UserId}", orders.Count(), userId);
        return Ok(orders);
    }

    [HttpGet("{orderId}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderResponse>> GetOrder([FromRoute] Guid orderId)
    {
        _logger.LogInformation("Getting order {OrderId}", orderId);
        var order = await _orderService.GetOrderAsync(orderId);
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            return NotFound(new { error = $"Order {orderId} not found" });
        }
        _logger.LogInformation("Order {OrderId} retrieved, status: {Status}", orderId, order.Status);
        return Ok(order);
    }
}

