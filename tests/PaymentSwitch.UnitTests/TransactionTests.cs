using FluentAssertions;
using PaymentSwitch.Domain.Entities;
using PaymentSwitch.Domain.Enums;
namespace PaymentSwitch.UnitTests;
public class TransactionTests
{
    [Fact]
    public void Create_ValidTransaction_ShouldHavePendingStatus()
    {
        var tx = Transaction.Create("idem-001", "1234567890", "HBL", "0987654321", "UBL", 5000m, TransactionType.InterBankTransfer);
        tx.Status.Should().Be(TransactionStatus.Pending);
        tx.Id.Should().NotBeEmpty();
        tx.SwitchReferenceId.Should().StartWith("SW-");
        tx.DomainEvents.Should().HaveCount(1);
    }
    [Fact]
    public void Create_NegativeAmount_ShouldThrow()
    {
        var act = () => Transaction.Create("idem-002", "1234567890", "HBL", "0987654321", "UBL", -100m, TransactionType.InterBankTransfer);
        act.Should().Throw<ArgumentException>().WithMessage("*positive*");
    }
    [Fact]
    public void Create_ExceedsLimit_ShouldThrow()
    {
        var act = () => Transaction.Create("idem-003", "1234567890", "HBL", "0987654321", "UBL", 6_000_000m, TransactionType.InterBankTransfer);
        act.Should().Throw<ArgumentException>().WithMessage("*5,000,000*");
    }
    [Fact]
    public void MarkProcessing_FromPending_ShouldSucceed()
    {
        var tx = Transaction.Create("idem-004", "1234567890", "HBL", "0987654321", "UBL", 1000m, TransactionType.InterBankTransfer);
        tx.MarkProcessing();
        tx.Status.Should().Be(TransactionStatus.Processing);
        tx.ProcessedAt.Should().NotBeNull();
    }
    [Fact]
    public void MarkCompleted_FromProcessing_ShouldSucceed()
    {
        var tx = Transaction.Create("idem-005", "1234567890", "HBL", "0987654321", "UBL", 1000m, TransactionType.InterBankTransfer);
        tx.MarkProcessing();
        tx.MarkCompleted();
        tx.Status.Should().Be(TransactionStatus.Completed);
        tx.CompletedAt.Should().NotBeNull();
        tx.DomainEvents.Should().HaveCount(2);
    }
    [Fact]
    public void MarkProcessing_AlreadyProcessing_ShouldThrow()
    {
        var tx = Transaction.Create("idem-006", "1234567890", "HBL", "0987654321", "UBL", 1000m, TransactionType.InterBankTransfer);
        tx.MarkProcessing();
        var act = () => tx.MarkProcessing();
        act.Should().Throw<InvalidOperationException>();
    }
    [Fact]
    public void MarkFailed_FromCompleted_ShouldThrow()
    {
        var tx = Transaction.Create("idem-007", "1234567890", "HBL", "0987654321", "UBL", 1000m, TransactionType.InterBankTransfer);
        tx.MarkProcessing();
        tx.MarkCompleted();
        var act = () => tx.MarkFailed("reason");
        act.Should().Throw<InvalidOperationException>();
    }
    [Fact]
    public void IdempotencyKey_ShouldBeStored()
    {
        var tx = Transaction.Create("my-key-123", "1234567890", "HBL", "0987654321", "UBL", 1000m, TransactionType.InterBankTransfer);
        tx.IdempotencyKey.Should().Be("my-key-123");
    }
}
