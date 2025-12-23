using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace HSEGozon.ApiGateway.Domain.DTOs;

public class TopUpAccountRequest
{
    [Required(ErrorMessage = "Amount обязателен")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Сумма должна быть больше 0")]
    [SwaggerSchema(Description = "Сумма пополнения счета (должна быть больше 0)")]
    public decimal Amount { get; set; }
}

