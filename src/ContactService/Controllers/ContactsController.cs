using ContactService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace ContactService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactsController(ContactDbContext context, ILogger<ContactsController> logger) : ControllerBase
{
  [HttpPost]
  public async Task<ActionResult<Contact>> CreateContact(CreateContactRequest request)
  {
    try
    {
      var contact = new Contact
      {
        FirstName = request.FirstName,
        LastName = request.LastName,
        Company = request.Company
      };

      context.Contacts.Add(contact);
      await context.SaveChangesAsync();

      logger.LogInformation("Contact created with ID: {ContactId}", contact.Id);
      return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error creating contact");
      return StatusCode(500, "An error occurred while creating the contact");
    }
  }

  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> DeleteContact(Guid id)
  {
    try
    {
      var contact = await context.Contacts.FindAsync(id);
      if (contact == null)
      {
        return NotFound();
      }

      context.Contacts.Remove(contact);
      await context.SaveChangesAsync();

      logger.LogInformation("Contact deleted with ID: {ContactId}", id);
      return NoContent();
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error deleting contact {ContactId}", id);
      return StatusCode(500, "An error occurred while deleting the contact");
    }
  }

  [HttpGet]
  public async Task<ActionResult<List<Contact>>> GetContacts()
  {
    try
    {
      var contacts = await context.Contacts
          .Include(c => c.ContactInfos)
          .OrderBy(c => c.FirstName)
          .ThenBy(c => c.LastName)
          .ToListAsync();

      return Ok(contacts);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error retrieving contacts");
      return StatusCode(500, "An error occurred while retrieving contacts");
    }
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<Contact>> GetContact(Guid id)
  {
    try
    {
      var contact = await context.Contacts
          .Include(c => c.ContactInfos)
          .FirstOrDefaultAsync(c => c.Id == id);

      if (contact == null)
      {
        return NotFound();
      }

      return Ok(contact);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error retrieving contact {ContactId}", id);
      return StatusCode(500, "An error occurred while retrieving the contact");
    }
  }

  [HttpPost("{contactId:guid}/contact-info")]
  public async Task<IActionResult> AddContactInfo(Guid contactId, AddContactInfoRequest request)
  {
    try
    {
      var contact = await context.Contacts.FindAsync(contactId);
      if (contact == null)
      {
        return NotFound("Contact not found");
      }

      var contactInfo = new ContactInfo
      {
        ContactId = contactId,
        InfoType = request.InfoType,
        InfoContent = request.InfoContent
      };

      context.ContactInfos.Add(contactInfo);
      await context.SaveChangesAsync();

      logger.LogInformation("Contact info added to contact {ContactId}", contactId);
      return CreatedAtAction(nameof(GetContact), new { id = contactId }, contactInfo);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error adding contact info to contact {ContactId}", contactId);
      return StatusCode(500, "An error occurred while adding contact information");
    }
  }

  [HttpDelete("contact-info/{contactInfoId:guid}")]
  public async Task<IActionResult> RemoveContactInfo(Guid contactInfoId)
  {
    try
    {
      var contactInfo = await context.ContactInfos.FindAsync(contactInfoId);
      if (contactInfo == null)
      {
        return NotFound();
      }

      context.ContactInfos.Remove(contactInfo);
      await context.SaveChangesAsync();

      logger.LogInformation("Contact info removed with ID: {ContactInfoId}", contactInfoId);
      return NoContent();
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error removing contact info {ContactInfoId}", contactInfoId);
      return StatusCode(500, "An error occurred while removing contact information");
    }
  }
}

// Request DTOs
public record CreateContactRequest(string FirstName, string LastName, string? Company = null);
public record AddContactInfoRequest(ContactInfoType InfoType, string InfoContent);