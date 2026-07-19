using Current.Api.DTOs.Contacts;

namespace Current.Api.Interfaces;

public interface IContactService
{
    Task<IReadOnlyList<ContactResponse>> GetAllContactsAsync(Guid currentUserId);

    Task<ContactResponse?> GetContactByIdAsync(Guid contactId, Guid currentUserId);

    Task<ContactResponse> CreateContactAsync(CreateContactRequest request, Guid currentUserId);

    Task<ContactResponse?> UpdateContactAsync(
        Guid contactId,
        UpdateContactRequest request,
        Guid currentUserId);

    Task<bool> DeleteContactAsync(Guid contactId, Guid currentUserId);
}
