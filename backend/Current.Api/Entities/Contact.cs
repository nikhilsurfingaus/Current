namespace Current.Api.Entities;

public class Contact
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Name { get; set; }

    public string? Email { get; set; }

    public string? Bsb { get; set; }

    public string? AccountNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
