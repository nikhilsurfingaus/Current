namespace Current.Api.Entities;

// Maps to the "Accounts" table in PostgreSQL
public class Account
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public AccountType AccountType { get; set; }

    public decimal CurrentBalance { get; set; }

    public string Currency { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
