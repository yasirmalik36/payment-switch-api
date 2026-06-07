namespace PaymentSwitch.Application.DTOs;
public record CreateTransactionRequest(
    string IdempotencyKey,
    string SenderAccountNumber,
    string SenderBankCode,
    string ReceiverAccountNumber,
    string ReceiverBankCode,
    decimal Amount,
    string TransactionType,
    string Currency = "PKR"
);
public record TransactionResponse(
    Guid Id,
    string SwitchReferenceId,
    string SenderAccountNumber,
    string SenderBankCode,
    string ReceiverAccountNumber,
    string ReceiverBankCode,
    decimal Amount,
    string Currency,
    string Status,
    string Type,
    DateTime CreatedAt,
    DateTime? ProcessedAt,
    DateTime? CompletedAt,
    string? FailureReason,
    int RetryCount
);
public record TransactionSummary(Guid Id, string SwitchReferenceId, decimal Amount, string Status, DateTime CreatedAt);
public record PagedResult<T>(IEnumerable<T> Items, int Total, int Page, int PageSize);
public record ApiResponse<T>(bool Success, T? Data, string? Error = null);