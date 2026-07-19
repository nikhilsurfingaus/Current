using Current.Api.Data;
using Current.Api.DTOs.Contacts;
using Current.Api.Entities;
using Current.Api.Interfaces;
using Current.Api.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Current.Api.Services;

public class ContactService : IContactService
{
    private readonly ApplicationDbContext _dbContext;

    public ContactService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ContactResponse>> GetAllContactsAsync(Guid currentUserId)
    {
        var contacts = await _dbContext.Contacts
            .AsNoTracking()
            .Where(contact => contact.UserId == currentUserId)
            .OrderBy(contact => contact.Name)
            .ThenBy(contact => contact.Email)
            .ToListAsync();

        return contacts.Select(contact => contact.ToResponse()).ToList();
    }

    public async Task<ContactResponse?> GetContactByIdAsync(Guid contactId, Guid currentUserId)
    {
        var contact = await _dbContext.Contacts
            .AsNoTracking()
            .FirstOrDefaultAsync(contact =>
                contact.Id == contactId && contact.UserId == currentUserId);

        return contact?.ToResponse();
    }

    public async Task<ContactResponse> CreateContactAsync(
        CreateContactRequest request,
        Guid currentUserId)
    {
        var contactName = NormalizeName(request.Name);
        var contactEmail = NormalizeEmail(request.Email);
        await EnsureEmailIsAvailableAsync(contactEmail, currentUserId);

        var utcNow = DateTime.UtcNow;
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Name = contactName,
            Email = contactEmail,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        return contact.ToResponse();
    }

    public async Task<ContactResponse?> UpdateContactAsync(
        Guid contactId,
        UpdateContactRequest request,
        Guid currentUserId)
    {
        var contact = await _dbContext.Contacts
            .FirstOrDefaultAsync(contact =>
                contact.Id == contactId && contact.UserId == currentUserId);

        if (contact is null)
        {
            return null;
        }

        var contactName = NormalizeName(request.Name);
        var contactEmail = NormalizeEmail(request.Email);
        await EnsureEmailIsAvailableAsync(contactEmail, currentUserId, contact.Id);

        contact.Name = contactName;
        contact.Email = contactEmail;
        contact.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return contact.ToResponse();
    }

    public async Task<bool> DeleteContactAsync(Guid contactId, Guid currentUserId)
    {
        var contact = await _dbContext.Contacts
            .FirstOrDefaultAsync(contact =>
                contact.Id == contactId && contact.UserId == currentUserId);

        if (contact is null)
        {
            return false;
        }

        _dbContext.Contacts.Remove(contact);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private async Task EnsureEmailIsAvailableAsync(
        string contactEmail,
        Guid currentUserId,
        Guid? excludedContactId = null)
    {
        var duplicateContactExists = await _dbContext.Contacts
            .AnyAsync(contact =>
                contact.UserId == currentUserId &&
                contact.Email == contactEmail &&
                contact.Id != excludedContactId);

        if (duplicateContactExists)
        {
            throw new InvalidOperationException("A contact with this email already exists.");
        }
    }

    private static string NormalizeName(string name)
    {
        var contactName = name.Trim();

        if (string.IsNullOrWhiteSpace(contactName))
        {
            throw new InvalidOperationException("Contact name is required.");
        }

        if (contactName.Length > 100)
        {
            throw new InvalidOperationException("Contact name must be 100 characters or fewer.");
        }

        return contactName;
    }

    private static string NormalizeEmail(string email)
    {
        var contactEmail = email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(contactEmail) || !contactEmail.Contains('@'))
        {
            throw new InvalidOperationException("A valid contact email is required.");
        }

        if (contactEmail.Length > 255)
        {
            throw new InvalidOperationException("Contact email must be 255 characters or fewer.");
        }

        return contactEmail;
    }
}
