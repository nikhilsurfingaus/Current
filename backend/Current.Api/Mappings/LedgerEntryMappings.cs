using Current.Api.DTOs.Transactions;
using Current.Api.Entities;

namespace Current.Api.Mappings;

public static class LedgerEntryMappings
{
    public static LedgerEntryResponse ToResponse(this LedgerEntry ledgerEntry)
    {
        return new LedgerEntryResponse
        {
            Id = ledgerEntry.Id,
            AccountId = ledgerEntry.AccountId,
            EntryType = ledgerEntry.EntryType,
            Amount = ledgerEntry.Amount,
            CreatedAt = ledgerEntry.CreatedAt
        };
    }
}
