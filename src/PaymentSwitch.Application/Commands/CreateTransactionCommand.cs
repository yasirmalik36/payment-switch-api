using MediatR;
using PaymentSwitch.Application.DTOs;
using PaymentSwitch.Domain.Entities;
using PaymentSwitch.Domain.Enums;
using PaymentSwitch.Domain.Interfaces;
namespace PaymentSwitch.Application.Commands;
public record CreateTransactionCommand(CreateTransactionRequest Request) : IRequest<TransactionResponse>;
public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionResponse>
{
    private readonly ITransactionRepository _repo;
    private readonly IMessagePublisher _publisher;
    public CreateTransactionCommandHandler(ITransactionRepository repo, IMessagePublisher publisher) { _repo = repo; _publisher = publisher; }
    public async Task<TransactionResponse> Handle(CreateTransactionCommand command, CancellationToken ct)
    {
        var req = command.Request;
        if (await _repo.ExistsByIdempotencyKeyAsync(req.IdempotencyKey, ct))
        {
            var existing = await _repo.GetByIdempotencyKeyAsync(req.IdempotencyKey, ct);
            return MapToResponse(existing!);
        }
        if (!Enum.TryParse<TransactionType>(req.TransactionType, out var txType))
            throw new ArgumentException($"Invalid transaction type: {req.TransactionType}");
        var transaction = Transaction.Create(req.IdempotencyKey, req.SenderAccountNumber, req.SenderBankCode, req.ReceiverAccountNumber, req.ReceiverBankCode, req.Amount, txType, req.Currency);
        await _repo.AddAsync(transaction, ct);
        await _publisher.PublishAsync("transactions.created", new { TransactionId = transaction.Id, transaction.Amount, transaction.SenderBankCode, transaction.ReceiverBankCode });
        return MapToResponse(transaction);
    }
    private static TransactionResponse MapToResponse(Transaction t) => new(t.Id, t.SwitchReferenceId ?? "", t.SenderAccountNumber, t.SenderBankCode, t.ReceiverAccountNumber, t.ReceiverBankCode, t.Amount, t.Currency, t.Status.ToString(), t.Type.ToString(), t.CreatedAt, t.ProcessedAt, t.CompletedAt, t.FailureReason, t.RetryCount);
}