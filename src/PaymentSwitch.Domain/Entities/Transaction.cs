using PaymentSwitch.Domain.Enums;
using PaymentSwitch.Domain.Events;
namespace PaymentSwitch.Domain.Entities;
public class Transaction
{
    public Guid Id { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;
    public string SenderAccountNumber { get; private set; } = default!;
    public string SenderBankCode { get; private set; } = default!;
    public string ReceiverAccountNumber { get; private set; } = default!;
    public string ReceiverBankCode { get; private set; } = default!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "PKR";
    public TransactionStatus Status { get; private set; }
    public TransactionType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int RetryCount { get; private set; }
    public string? SwitchReferenceId { get; private set; }
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    private Transaction() { }
    public static Transaction Create(string idempotencyKey, string senderAccount, string senderBank, string receiverAccount, string receiverBank, decimal amount, TransactionType type, string currency = "PKR")
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive", nameof(amount));
        if (amount > 5_000_000) throw new ArgumentException("Exceeds PKR 5,000,000 limit", nameof(amount));
        var tx = new Transaction { Id = Guid.NewGuid(), IdempotencyKey = idempotencyKey, SenderAccountNumber = senderAccount, SenderBankCode = senderBank, ReceiverAccountNumber = receiverAccount, ReceiverBankCode = receiverBank, Amount = amount, Currency = currency, Type = type, Status = TransactionStatus.Pending, CreatedAt = DateTime.UtcNow, SwitchReferenceId = $"SW-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}" };
        tx._domainEvents.Add(new TransactionCreatedEvent(tx.Id, tx.Amount, senderBank, receiverBank));
        return tx;
    }
    public void MarkProcessing() { if (Status != TransactionStatus.Pending) throw new InvalidOperationException($"Cannot process in status: {Status}"); Status = TransactionStatus.Processing; ProcessedAt = DateTime.UtcNow; }
    public void MarkCompleted() { if (Status != TransactionStatus.Processing) throw new InvalidOperationException($"Cannot complete in status: {Status}"); Status = TransactionStatus.Completed; CompletedAt = DateTime.UtcNow; _domainEvents.Add(new TransactionCompletedEvent(Id, Amount, SenderBankCode, ReceiverBankCode)); }
    public void MarkFailed(string reason) { if (Status == TransactionStatus.Completed) throw new InvalidOperationException("Cannot fail completed tx"); Status = TransactionStatus.Failed; FailureReason = reason; CompletedAt = DateTime.UtcNow; _domainEvents.Add(new TransactionFailedEvent(Id, reason)); }
    public void IncrementRetry() { RetryCount++; Status = TransactionStatus.Pending; }
    public void ClearDomainEvents() => _domainEvents.Clear();
}