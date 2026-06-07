using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentSwitch.Application.Commands;
using PaymentSwitch.Application.DTOs;
using PaymentSwitch.Application.Queries;
namespace PaymentSwitch.API.Controllers;
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TransactionsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Create a new payment transaction (idempotent)</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TransactionResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken ct)
    {
        if (!Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
            return BadRequest(new ApiResponse<object>(false, null, "Idempotency-Key header is required"));
        var cmd = new CreateTransactionCommand(request with { IdempotencyKey = idempotencyKey.ToString() });
        var result = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new ApiResponse<TransactionResponse>(true, result));
    }

    /// <summary>Get transaction by ID</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TransactionResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTransactionQuery(id), ct);
        return Ok(new ApiResponse<TransactionResponse>(true, result));
    }

    /// <summary>Process a pending transaction (trigger routing)</summary>
    [HttpPost("{id:guid}/process")]
    [ProducesResponseType(typeof(ApiResponse<TransactionResponse>), 200)]
    public async Task<IActionResult> Process(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ProcessTransactionCommand(id), ct);
        return Ok(new ApiResponse<TransactionResponse>(true, result));
    }
}