using HSEGozon.ApiGateway.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace HSEGozon.ApiGateway.Api.Controllers;

[ApiController]
[Route("api")]
public class RoutingController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<RoutingController> _logger;

    public RoutingController(IHttpClientFactory httpClientFactory, ILogger<RoutingController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpPost("orders")]
    [SwaggerRequestExample(typeof(CreateOrderRequest), typeof(Api.Examples.CreateOrderRequestExample))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        return await ProxyRequestAsync("OrdersService", "api/orders", HttpMethod.Post, request);
    }

    [HttpGet("orders/user/{userId}")]
    public async Task<IActionResult> GetOrders([FromRoute] Guid userId)
    {
        return await ProxyRequestAsync("OrdersService", $"api/orders/user/{userId}", HttpMethod.Get, null);
    }

    [HttpGet("orders/{orderId}")]
    public async Task<IActionResult> GetOrder([FromRoute] Guid orderId)
    {
        return await ProxyRequestAsync("OrdersService", $"api/orders/{orderId}", HttpMethod.Get, null);
    }

    [HttpPost("accounts")]
    [SwaggerRequestExample(typeof(CreateAccountRequest), typeof(Api.Examples.CreateAccountRequestExample))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        return await ProxyRequestAsync("PaymentsService", "api/accounts", HttpMethod.Post, request);
    }

    [HttpPost("accounts/{userId}/topup")]
    [SwaggerRequestExample(typeof(TopUpAccountRequest), typeof(Api.Examples.TopUpAccountRequestExample))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TopUpAccount([FromRoute] Guid userId, [FromBody] TopUpAccountRequest request)
    {
        return await ProxyRequestAsync("PaymentsService", $"api/accounts/{userId}/topup", HttpMethod.Post, request);
    }

    [HttpGet("accounts/{userId}")]
    public async Task<IActionResult> GetAccount([FromRoute] Guid userId)
    {
        return await ProxyRequestAsync("PaymentsService", $"api/accounts/{userId}", HttpMethod.Get, null);
    }

    private async Task<IActionResult> ProxyRequestAsync(string clientName, string path, HttpMethod method, object? body)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(clientName);
            var request = new HttpRequestMessage(method, path);

            if (body != null)
            {
                var json = System.Text.Json.JsonSerializer.Serialize(body);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }

            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = content,
                ContentType = "application/json"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying request to {ClientName}/{Path}", clientName, path);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

