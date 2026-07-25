using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Accounts;

public class AccountResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public AccountType AccountType { get; set; }

    public decimal CurrentBalance { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string Bsb { get; set; } = string.Empty;

    public string AccountNumber { get; set; } = string.Empty;

    public decimal? WelcomeCreditAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
