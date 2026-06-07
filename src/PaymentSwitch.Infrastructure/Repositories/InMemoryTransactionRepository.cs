using PaymentSwitch.Domain.Entities;
using PaymentSwitch.Domain.Enums;
using PaymentSwitch.Domain.Interfaces;
using System.Collections.Concurrent;
namespace PaymentSwitch.Infrastructure.Repositories;
/// <summary>
/// Thread-safe in-memory repository for demo/testing.
/// Replace with EF Core + SQL Server for production.
/// </summary>
public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly ConcurrentDictionary<Guid, Transaction> _store = new();
    private readonly ConcurrentDictionary<string, Guid> _idempotencyIndex = new();
    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult(_store.TryGetValue(id, out var tx) ? tx : null);
    public Task<Transaction?> GetByIdempotencyKeyAsync(string key, CancellationToken ct = default) => Task.FromResult(_idempotencyIndex.TryGetValue(key, out var id) && _store.TryGetValue(id, out var tx) ? tx : null);
    public Task<IEnumerable<Transaction>> GetByStatusAsync(TransactionStatus status, int limit = 100, CancellationToken ct = default) => Task.FromResult(_store.Values.Where(t => t.Status == status).Take(limit));
    public Task AddAsync(Transaction transaction, CancellationToken ct = default) { _store[transaction.Id] = transaction; _idempotencyIndex[transaction.IdempotencyKey] = transaction.Id; return Task.CompletedTask; }
    public Task UpdateAsync(Transaction transaction, CancellationToken ct = default) { _store[transaction.Id] = transaction; return Task.CompletedTask; }
    public Task<bool> ExistsByIdempotencyKeyAsync(string key, CancellationToken ct = default) => Task.FromResult(_idempotencyIndex.ContainsKey(key));
}