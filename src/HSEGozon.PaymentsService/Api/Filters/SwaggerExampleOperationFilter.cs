using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using HSEGozon.PaymentsService.Domain.DTOs;

namespace HSEGozon.PaymentsService.Api.Filters;

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
            
            if (parameterType == typeof(CreateAccountRequest))
            {
                foreach (var content in operation.RequestBody.Content.Values)
                {
                    content.Example = new OpenApiObject
                    {
                        ["userId"] = new OpenApiString("123e4567-e89b-12d3-a456-426614174000")
                    };
                }
                break;
            }
            else if (parameterType == typeof(TopUpAccountRequest))
            {
                foreach (var content in operation.RequestBody.Content.Values)
                {
                    content.Example = new OpenApiObject
                    {
                        ["amount"] = new OpenApiDouble(1000)
                    };
                }
                break;
            }
        }
    }
}

