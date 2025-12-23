using Swashbuckle.AspNetCore.Filters;
using HSEGozon.ApiGateway.Domain.DTOs;

namespace HSEGozon.ApiGateway.Api.Examples;

public class CreateOrderRequestExample : IExamplesProvider<CreateOrderRequest>
{
    public CreateOrderRequest GetExamples()
    {
        return new CreateOrderRequest
        {
            UserId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000"),
            Amount = 500.00m,
            Description = "Ноутбук ASUS ROG Strix"
        };
    }
}

