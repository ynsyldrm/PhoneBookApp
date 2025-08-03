using Shared.Models;

namespace ReportService.Services;

public interface IContactServiceClient
{
  Task<List<Contact>> GetAllContactsAsync();
  Task<Contact?> GetContactAsync(Guid id);
}