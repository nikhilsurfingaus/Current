namespace Current.Api.Entities;

// Maps to the "Users" table in PostgreSQL
public class User
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation property — EF Core uses this for the 1:many relationship
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
