using Current.Api.DTOs.Accounts;

namespace Current.Api.Interfaces;

public interface IAccountService
{
    Task<IReadOnlyList<AccountResponse>> GetAllAccountsAsync(Guid currentUserId);

    Task<AccountResponse?> GetAccountByIdAsync(Guid accountId, Guid currentUserId);

    Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request, Guid currentUserId);
}
