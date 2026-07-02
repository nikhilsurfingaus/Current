using Current.Api.Common.Enums;

namespace Current.Api.Entities;

public class Transaction
{
    public Guid Id { get; set; }

    public Guid FromAccountId { get; set; }

    public Guid ToAccountId { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public TransactionStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public Account FromAccount { get; set; } = null!;

    public Account ToAccount { get; set; } = null!;

    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
}
