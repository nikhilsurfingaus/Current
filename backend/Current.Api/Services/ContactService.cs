using Current.Api.Common;
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
        var contactDetails = NormalizeContactDetails(request.Name, request.Email, request.Bsb, request.AccountNumber);
        await EnsureContactIsAvailableAsync(
            currentUserId,
            contactDetails.Email,
            contactDetails.Bsb,
            contactDetails.AccountNumber);

        var utcNow = DateTime.UtcNow;
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Name = contactDetails.Name,
            Email = contactDetails.Email,
            Bsb = contactDetails.Bsb,
            AccountNumber = contactDetails.AccountNumber,
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

        var contactDetails = NormalizeContactDetails(request.Name, request.Email, request.Bsb, request.AccountNumber);
        await EnsureContactIsAvailableAsync(
            currentUserId,
            contactDetails.Email,
            contactDetails.Bsb,
            contactDetails.AccountNumber,
            contact.Id);

        contact.Name = contactDetails.Name;
        contact.Email = contactDetails.Email;
        contact.Bsb = contactDetails.Bsb;
        contact.AccountNumber = contactDetails.AccountNumber;
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

    private async Task EnsureContactIsAvailableAsync(
        Guid currentUserId,
        string? contactEmail,
        string? contactBsb,
        string? contactAccountNumber,
        Guid? excludedContactId = null)
    {
        if (!string.IsNullOrWhiteSpace(contactEmail))
        {
            var duplicateEmailContactExists = await _dbContext.Contacts
                .AnyAsync(contact =>
                    contact.UserId == currentUserId &&
                    contact.Email == contactEmail &&
                    contact.Id != excludedContactId);

            if (duplicateEmailContactExists)
            {
                throw new InvalidOperationException("A contact with this email already exists.");
            }
        }

        if (!string.IsNullOrWhiteSpace(contactBsb) && !string.IsNullOrWhiteSpace(contactAccountNumber))
        {
            var duplicateBankContactExists = await _dbContext.Contacts
                .AnyAsync(contact =>
                    contact.UserId == currentUserId &&
                    contact.Bsb == contactBsb &&
                    contact.AccountNumber == contactAccountNumber &&
                    contact.Id != excludedContactId);

            if (duplicateBankContactExists)
            {
                throw new InvalidOperationException("A contact with these bank details already exists.");
            }
        }
    }

    private static ContactDetails NormalizeContactDetails(
        string name,
        string? email,
        string? bsb,
        string? accountNumber)
    {
        var contactName = NormalizeName(name);
        var hasEmail = !string.IsNullOrWhiteSpace(email);
        var hasBsbDetails = !string.IsNullOrWhiteSpace(bsb) || !string.IsNullOrWhiteSpace(accountNumber);

        if (!hasEmail && !hasBsbDetails)
        {
            throw new InvalidOperationException("Provide an email or BSB and account number.");
        }

        string? normalizedEmail = null;
        string? normalizedBsb = null;
        string? normalizedAccountNumber = null;

        if (hasEmail)
        {
            normalizedEmail = NormalizeEmail(email!);
        }

        if (hasBsbDetails)
        {
            if (!BankAccountNormalizer.TryNormalizeBsb(bsb, out normalizedBsb))
            {
                throw new InvalidOperationException("BSB must be 6 digits.");
            }

            if (!BankAccountNormalizer.TryNormalizeAccountNumber(accountNumber, out normalizedAccountNumber))
            {
                throw new InvalidOperationException("Account number must be 6 to 9 digits.");
            }
        }

        return new ContactDetails(contactName, normalizedEmail, normalizedBsb, normalizedAccountNumber);
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

    private sealed record ContactDetails(
        string Name,
        string? Email,
        string? Bsb,
        string? AccountNumber);
}
