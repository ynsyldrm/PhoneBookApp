using Shared.Models;

namespace ReportService.Services;

public class ContactServiceClient : IContactServiceClient
{
  private readonly HttpClient _httpClient;
  private readonly ILogger<ContactServiceClient> _logger;

  public ContactServiceClient(HttpClient httpClient, ILogger<ContactServiceClient> logger)
  {
    _httpClient = httpClient;
    _logger = logger;
  }

  public async Task<List<Contact>> GetAllContactsAsync()
  {
    try
    {
      var response = await _httpClient.GetAsync("/api/contacts");
      response.EnsureSuccessStatusCode();

      var contacts = await response.Content.ReadFromJsonAsync<List<Contact>>();
      return contacts ?? new List<Contact>();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error fetching contacts from Contact Service");
      throw;
    }
  }

  public async Task<Contact?> GetContactAsync(Guid id)
  {
    try
    {
      var response = await _httpClient.GetAsync($"/api/contacts/{id}");
      if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
      {
        return null;
      }

      response.EnsureSuccessStatusCode();
      return await response.Content.ReadFromJsonAsync<Contact>();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error fetching contact {ContactId} from Contact Service", id);
      throw;
    }
  }
}