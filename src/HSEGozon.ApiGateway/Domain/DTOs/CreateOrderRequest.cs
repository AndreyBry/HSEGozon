using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace HSEGozon.ApiGateway.Domain.DTOs;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "UserId обязателен")]
    [SwaggerSchema(Description = "Уникальный идентификатор пользователя в формате GUID")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Amount обязателен")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Сумма должна быть больше 0")]
    [SwaggerSchema(Description = "Сумма заказа (должна быть больше 0)")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Description обязателен")]
    [MinLength(1, ErrorMessage = "Описание не может быть пустым")]
    [SwaggerSchema(Description = "Описание заказа")]
    public string Description { get; set; } = string.Empty;
}

