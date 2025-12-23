namespace HSEGozon.PaymentsService.Domain.DTOs;

public class AccountResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public decimal Balance { get; set; }

    public DateTime CreatedAt { get; set; }
}

