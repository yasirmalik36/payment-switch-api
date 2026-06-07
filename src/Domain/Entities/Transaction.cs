// Domain/Entities/Transaction.cs
namespace PaymentSwitch.Domain.Entities;

public enum TransactionStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Reversed,
    TimedOut
}

public enum TransactionType
{
    Interbank,
    QRPayment,
    BillPayment,
    FundTransfer
}

/// <summary>
/// Core payment transaction aggregate root.
/// Immutable after creation — state changes via domain methods only.
/// </summary>
public class Transaction
{
    public Guid Id { get; private set; }
    public string IdempotencyKey { get; private set; }   // Client-supplied — prevents duplicates
    public string FromAccountIban { get; private set; }
    public string ToAccountIban { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public string? ReferenceNumber { get; private set; }  // Bank reference
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public List<TransactionEvent> Events { get; private set; } = new();

    // EF Core constructor
    private Transaction() { }

    public static Transaction Create(
        string idempotencyKey,
        string fromAccountIban,
        string toAccountIban,
        decimal amount,
        string currency,
        TransactionType type)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("Idempotency key is required", nameof(idempotencyKey));

        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            FromAccountIban = fromAccountIban.Trim().ToUpperInvariant(),
            ToAccountIban = toAccountIban.Trim().ToUpperInvariant(),
            Amount = Math.Round(amount, 2),
            Currency = currency.ToUpperInvariant(),
            Type = type,
            Status = TransactionStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        tx.AddEvent("CREATED", $"Transaction created for {amount} {currency}");
        return tx;
    }

    public void MarkProcessing()
    {
        EnsureStatus(TransactionStatus.Pending);
        Status = TransactionStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
        AddEvent("PROCESSING", "Transaction submitted to payment network");
    }

    public void MarkCompleted(string referenceNumber)
    {
        EnsureStatus(TransactionStatus.Processing);
        Status = TransactionStatus.Completed;
        ReferenceNumber = referenceNumber;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AddEvent("COMPLETED", $"Transaction completed. Ref: {referenceNumber}");
    }

    public void MarkFailed(string reason)
    {
        if (Status == TransactionStatus.Completed)
            throw new InvalidOperationException("Cannot fail a completed transaction");

        Status = TransactionStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
        AddEvent("FAILED", $"Transaction failed: {reason}");
    }

    public void MarkTimedOut()
    {
        if (Status == TransactionStatus.Completed) return;
        Status = TransactionStatus.TimedOut;
        FailureReason = "Payment network timeout";
        UpdatedAt = DateTime.UtcNow;
        AddEvent("TIMEOUT", "Transaction timed out waiting for network response");
    }

    private void EnsureStatus(TransactionStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException(
                $"Invalid status transition: {Status} → cannot perform this action");
    }

    private void AddEvent(string eventType, string description)
    {
        Events.Add(new TransactionEvent
        {
            Id = Guid.NewGuid(),
            TransactionId = Id,
            EventType = eventType,
            Description = description,
            OccurredAt = DateTime.UtcNow,
        });
    }
}

public class TransactionEvent
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
