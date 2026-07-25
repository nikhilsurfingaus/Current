using Current.Api.Entities;

namespace Current.Api.Interfaces;

public interface IBankAccountNumberService
{
    Task AssignBankDetailsAsync(Account account);

    Task AssignBranchTreasuryDetailsAsync(Account treasuryAccount);
}
