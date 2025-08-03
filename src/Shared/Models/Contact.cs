namespace Shared.Models
{
  public class Contact
  {
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Company { get; set; }
    public List<ContactInfo> ContactInfos { get; set; } = new();
  }

  public class ContactInfo
  {
    public Guid Id { get; set; }
    public Guid ContactId { get; set; }
    public ContactInfoType InfoType { get; set; }
    public string InfoContent { get; set; }
  }

  public enum ContactInfoType
  {
    PhoneNumber,
    EmailAddress,
    Location
  }
}
