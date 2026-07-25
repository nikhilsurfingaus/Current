namespace Current.Api.DTOs.Contacts;

public class CreateContactRequest
{
    public required string Name { get; set; }

    public string? Email { get; set; }

    public string? Bsb { get; set; }

    public string? AccountNumber { get; set; }
}
