namespace Current.Api.DTOs.Contacts;

public class UpdateContactRequest
{
    public required string Name { get; set; }

    public required string Email { get; set; }
}
