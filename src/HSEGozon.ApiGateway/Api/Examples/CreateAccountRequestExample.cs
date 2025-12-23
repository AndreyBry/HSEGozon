using Swashbuckle.AspNetCore.Filters;
using HSEGozon.ApiGateway.Domain.DTOs;

namespace HSEGozon.ApiGateway.Api.Examples;

public class CreateAccountRequestExample : IExamplesProvider<CreateAccountRequest>
{
    public CreateAccountRequest GetExamples()
    {
        return new CreateAccountRequest
        {
            UserId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000")
        };
    }
}

