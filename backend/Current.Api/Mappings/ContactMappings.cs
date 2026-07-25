using Current.Api.DTOs.Contacts;
using Current.Api.Entities;

namespace Current.Api.Mappings;

public static class ContactMappings
{
    public static ContactResponse ToResponse(this Contact contact)
    {
        return new ContactResponse
        {
            Id = contact.Id,
            Name = contact.Name,
            Email = contact.Email,
            Bsb = contact.Bsb,
            AccountNumber = contact.AccountNumber,
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.UpdatedAt
        };
    }
}
