namespace Current.Api.DTOs.Contacts;

public class ContactResponse
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string? Email { get; set; }

    public string? Bsb { get; set; }

    public string? AccountNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
