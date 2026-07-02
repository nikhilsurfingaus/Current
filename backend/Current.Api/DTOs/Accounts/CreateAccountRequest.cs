using Current.Api.Common.Enums;

namespace Current.Api.DTOs.Accounts;

public class CreateAccountRequest
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public AccountType AccountType { get; set; }

    public decimal CurrentBalance { get; set; }

    public string Currency { get; set; } = string.Empty;
}
