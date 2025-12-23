using Swashbuckle.AspNetCore.Filters;
using HSEGozon.PaymentsService.Domain.DTOs;

namespace HSEGozon.PaymentsService.Api.Examples;

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

