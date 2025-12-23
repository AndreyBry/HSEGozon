using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using HSEGozon.OrdersService.Domain.DTOs;

namespace HSEGozon.OrdersService.Api.Filters;

public class SwaggerExampleOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content == null)
            return;

        var parameters = context.MethodInfo.GetParameters();
        foreach (var parameter in parameters)
        {
            var parameterType = parameter.ParameterType;
            
            if (parameterType == typeof(CreateOrderRequest))
            {
                foreach (var content in operation.RequestBody.Content.Values)
                {
                    content.Example = new OpenApiObject
                    {
                        ["userId"] = new OpenApiString("123e4567-e89b-12d3-a456-426614174000"),
                        ["amount"] = new OpenApiDouble(500.00),
                        ["description"] = new OpenApiString("Ноутбук ASUS ROG Strix")
                    };
                }
                break;
            }
        }
    }
}

