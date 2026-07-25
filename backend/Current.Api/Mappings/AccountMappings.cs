using Current.Api.DTOs.Accounts;
using Current.Api.Entities;

namespace Current.Api.Mappings;

public static class AccountMappings
{
    public static AccountResponse ToResponse(this Account account)
    {
        return new AccountResponse
        {
            Id = account.Id,
            UserId = account.UserId,
            Name = account.Name,
            AccountType = account.AccountType,
            CurrentBalance = account.CurrentBalance,
            Currency = account.Currency,
            Bsb = account.Bsb,
            AccountNumber = account.AccountNumber,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };
    }
}
