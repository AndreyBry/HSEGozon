using HSEGozon.PaymentsService.Abstractions;
using HSEGozon.PaymentsService.Api.Examples;
using HSEGozon.PaymentsService.Domain.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace HSEGozon.PaymentsService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    [HttpPost]
    [SwaggerRequestExample(typeof(CreateAccountRequest), typeof(CreateAccountRequestExample))]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountResponse>> CreateAccount([FromBody] CreateAccountRequest? request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required. Expected JSON object: {\"userId\": \"guid\"}" });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();
            
            return BadRequest(new { 
                error = "Validation failed", 
                details = errors,
                example = new { userId = "123e4567-e89b-12d3-a456-426614174000" }
            });
        }

        try
        {
            _logger.LogInformation("Creating account for user {UserId}", request.UserId);
            var account = await _accountService.CreateAccountAsync(request.UserId);
            if (account == null)
            {
                _logger.LogWarning("Failed to create account for user {UserId}", request.UserId);
                return BadRequest(new { error = "Failed to create account" });
            }
            _logger.LogInformation("Account created successfully for user {UserId}, account ID: {AccountId}", 
                request.UserId, account.Id);
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating account for user {UserId}", request.UserId);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost("{userId}/topup")]
    [SwaggerRequestExample(typeof(TopUpAccountRequest), typeof(TopUpAccountRequestExample))]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AccountResponse>> TopUpAccount(
        [FromRoute] Guid userId,
        [FromBody] TopUpAccountRequest request)
    {
        try
        {
            _logger.LogInformation("Topping up account for user {UserId} with amount {Amount}", 
                userId, request.Amount);
            var account = await _accountService.TopUpAccountAsync(userId, request.Amount);
            if (account == null)
            {
                _logger.LogWarning("Account not found for user {UserId} during top-up", userId);
                return NotFound(new { error = $"Account not found for user {userId}" });
            }
            _logger.LogInformation("Account topped up successfully for user {UserId}, new balance: {Balance}", 
                userId, account.Balance);
            return Ok(account);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument for top-up operation for user {UserId}", userId);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error topping up account for user {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> GetAccount([FromRoute] Guid userId)
    {
        _logger.LogInformation("Getting account for user {UserId}", userId);
        var account = await _accountService.GetAccountAsync(userId);
        if (account == null)
        {
            _logger.LogWarning("Account not found for user {UserId}", userId);
            return NotFound(new { error = $"Account not found for user {userId}" });
        }
        _logger.LogInformation("Account retrieved for user {UserId}, balance: {Balance}", 
            userId, account.Balance);
        return Ok(account);
    }
}

