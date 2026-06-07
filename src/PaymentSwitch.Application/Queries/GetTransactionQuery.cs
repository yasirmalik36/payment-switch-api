using MediatR;
using PaymentSwitch.Application.DTOs;
using PaymentSwitch.Domain.Interfaces;
namespace PaymentSwitch.Application.Queries;
public record GetTransactionQuery(Guid TransactionId) : IRequest<TransactionResponse>;
public class GetTransactionQueryHandler : IRequestHandler<GetTransactionQuery, TransactionResponse>
{
    private readonly ITransactionRepository _repo;
    public GetTransactionQueryHandler(ITransactionRepository repo) => _repo = repo;
    public async Task<TransactionResponse> Handle(GetTransactionQuery request, CancellationToken ct)
    {
        var tx = await _repo.GetByIdAsync(request.TransactionId, ct) ?? throw new KeyNotFoundException($"Transaction {request.TransactionId} not found");
        return new TransactionResponse(tx.Id, tx.SwitchReferenceId ?? "", tx.SenderAccountNumber, tx.SenderBankCode, tx.ReceiverAccountNumber, tx.ReceiverBankCode, tx.Amount, tx.Currency, tx.Status.ToString(), tx.Type.ToString(), tx.CreatedAt, tx.ProcessedAt, tx.CompletedAt, tx.FailureReason, tx.RetryCount);
    }
}