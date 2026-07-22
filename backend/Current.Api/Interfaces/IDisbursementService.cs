using Current.Api.Common.Enums;
using Current.Api.Entities;

namespace Current.Api.Interfaces;

public interface IDisbursementService
{
    Task<Branch> GetDefaultBranchAsync();

    Task ApplyDisbursementAsync(
        Account treasuryAccount,
        Account recipientAccount,
        decimal amount,
        string description,
        TransactionCategory category);
}
