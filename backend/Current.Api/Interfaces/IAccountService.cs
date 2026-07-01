using Current.Api.DTOs.Accounts;

namespace Current.Api.Interfaces;

// Contract for account business logic — keeps controllers decoupled from EF Core
public interface IAccountService
{
    Task<IReadOnlyList<AccountResponse>> GetAllAccountsAsync();

    Task<AccountResponse?> GetAccountByIdAsync(Guid accountId);

    Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request);
}
