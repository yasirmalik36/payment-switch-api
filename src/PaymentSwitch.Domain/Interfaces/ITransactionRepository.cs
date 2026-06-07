using PaymentSwitch.Domain.Entities;
using PaymentSwitch.Domain.Enums;
namespace PaymentSwitch.Domain.Interfaces;
public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Transaction?> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default);
    Task<IEnumerable<Transaction>> GetByStatusAsync(TransactionStatus status, int limit = 100, CancellationToken ct = default);
    Task AddAsync(Transaction transaction, CancellationToken ct = default);
    Task UpdateAsync(Transaction transaction, CancellationToken ct = default);
    Task<bool> ExistsByIdempotencyKeyAsync(string key, CancellationToken ct = default);
}