using Current.Api.DTOs.Contacts;
using Current.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Current.Api.Controllers;

[Authorize]
[ApiController]
[Route("contacts")]
public class ContactsController : ControllerBase
{
    private readonly IContactService _contactService;
    private readonly ICurrentUserService _currentUserService;

    public ContactsController(
        IContactService contactService,
        ICurrentUserService currentUserService)
    {
        _contactService = contactService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContactResponse>>> GetAll()
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var contacts = await _contactService.GetAllContactsAsync(currentUserId);
        return Ok(contacts);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContactResponse>> GetById(Guid id)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var contact = await _contactService.GetContactByIdAsync(id, currentUserId);

        if (contact is null)
        {
            return NotFound();
        }

        return Ok(contact);
    }

    [HttpPost]
    public async Task<ActionResult<ContactResponse>> Create([FromBody] CreateContactRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var contact = await _contactService.CreateContactAsync(request, currentUserId);
            return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ContactResponse>> Update(
        Guid id,
        [FromBody] UpdateContactRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetCurrentUserId();
            var contact = await _contactService.UpdateContactAsync(id, request, currentUserId);

            if (contact is null)
            {
                return NotFound();
            }

            return Ok(contact);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        var contactDeleted = await _contactService.DeleteContactAsync(id, currentUserId);

        if (!contactDeleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
