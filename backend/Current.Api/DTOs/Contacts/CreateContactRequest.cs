namespace Current.Api.DTOs.Contacts;

public class CreateContactRequest
{
    public required string Name { get; set; }

    public required string Email { get; set; }
}
