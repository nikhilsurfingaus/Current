using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Transactions;

public class LedgerEntryResponse
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public LedgerEntryType EntryType { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }
}
