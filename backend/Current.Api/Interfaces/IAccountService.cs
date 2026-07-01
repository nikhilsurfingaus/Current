using Current.Api.DTOs.Accounts;

namespace Current.Api.Interfaces;

public interface IAccountService
{
    Task<IReadOnlyList<AccountResponse>> GetAllAccountsAsync();

    Task<AccountResponse?> GetAccountByIdAsync(Guid accountId);

    Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request);
}
