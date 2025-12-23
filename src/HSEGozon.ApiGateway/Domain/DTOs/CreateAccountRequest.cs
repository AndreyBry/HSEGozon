using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace HSEGozon.ApiGateway.Domain.DTOs;

public class CreateAccountRequest
{
    [Required(ErrorMessage = "UserId обязателен")]
    [SwaggerSchema(Description = "Уникальный идентификатор пользователя в формате GUID")]
    public Guid UserId { get; set; }
}

