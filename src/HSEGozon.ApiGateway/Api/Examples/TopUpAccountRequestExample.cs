using Swashbuckle.AspNetCore.Filters;
using HSEGozon.ApiGateway.Domain.DTOs;

namespace HSEGozon.ApiGateway.Api.Examples;

public class TopUpAccountRequestExample : IExamplesProvider<TopUpAccountRequest>
{
    public TopUpAccountRequest GetExamples()
    {
        return new TopUpAccountRequest
        {
            Amount = 1000.50m
        };
    }
}

