using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class Contact
{
  public Guid Id { get; set; } = Guid.NewGuid();

  [Required]
  [StringLength(100)]
  public required string FirstName { get; set; }

  [Required]
  [StringLength(100)]
  public required string LastName { get; set; }

  [StringLength(200)]
  public string? Company { get; set; }

  public List<ContactInfo> ContactInfos { get; set; } = [];

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ContactInfo
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid ContactId { get; set; }
  public ContactInfoType InfoType { get; set; }

  [Required]
  [StringLength(500)]
  public required string InfoContent { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ContactInfoType
{
  PhoneNumber,
  EmailAddress,
  Location
}