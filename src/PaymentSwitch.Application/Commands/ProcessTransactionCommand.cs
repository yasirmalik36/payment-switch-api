using MediatR;
using PaymentSwitch.Application.DTOs;
using PaymentSwitch.Domain.Interfaces;
namespace PaymentSwitch.Application.Commands;
public record ProcessTransactionCommand(Guid TransactionId) : IRequest<TransactionResponse>;
public class ProcessTransactionCommandHandler : IRequestHandler<ProcessTransactionCommand, TransactionResponse>
{
    private readonly ITransactionRepository _repo;
    private readonly IMessagePublisher _publisher;
    public ProcessTransactionCommandHandler(ITransactionRepository repo, IMessagePublisher publisher) { _repo = repo; _publisher = publisher; }
    public async Task<TransactionResponse> Handle(ProcessTransactionCommand command, CancellationToken ct)
    {
        var tx = await _repo.GetByIdAsync(command.TransactionId, ct) ?? throw new KeyNotFoundException($"Transaction {command.TransactionId} not found");
        tx.MarkProcessing();
        await _repo.UpdateAsync(tx, ct);
        // Simulate bank routing (replace with actual bank API calls)
        await Task.Delay(Random.Shared.Next(100, 500), ct);
        var success = Random.Shared.NextDouble() > 0.05; // 95% success rate simulation
        if (success) { tx.MarkCompleted(); await _publisher.PublishAsync("transactions.completed", new { tx.Id, tx.Amount }); }
        else { tx.MarkFailed("Bank declined: insufficient funds"); await _publisher.PublishAsync("transactions.failed", new { tx.Id, tx.FailureReason }); }
        await _repo.UpdateAsync(tx, ct);
        return new TransactionResponse(tx.Id, tx.SwitchReferenceId ?? "", tx.SenderAccountNumber, tx.SenderBankCode, tx.ReceiverAccountNumber, tx.ReceiverBankCode, tx.Amount, tx.Currency, tx.Status.ToString(), tx.Type.ToString(), tx.CreatedAt, tx.ProcessedAt, tx.CompletedAt, tx.FailureReason, tx.RetryCount);
    }
}