using Microsoft.EntityFrameworkCore;
using ReportService.Data;
using Shared.Interfaces;
using Shared.Models;
using System.Text.Json;

namespace ReportService.Services;

public class ReportService(
    ReportDbContext context,
    IKafkaProducer kafkaProducer,
    IContactServiceClient contactServiceClient,
    ILogger<ReportService> logger) : IReportService
{
  public async Task<Guid> RequestReportAsync()
  {
    var report = new Report();

    context.Reports.Add(report);
    await context.SaveChangesAsync();

    // Kafka'ya mesaj gönder
    var message = new ReportRequestMessage(report.Id);
    await kafkaProducer.ProduceAsync("report-requests", JsonSerializer.Serialize(message));

    logger.LogInformation("Report request queued: {ReportId}", report.Id);
    return report.Id;
  }

  public async Task<List<Report>> GetReportsAsync()
  {
    return await context.Reports
        .OrderByDescending(r => r.RequestedDate)
        .ToListAsync();
  }

  public async Task<Report?> GetReportAsync(Guid id)
  {
    return await context.Reports.FindAsync(id);
  }

  public async Task ProcessReportAsync(Guid reportId)
  {
    var report = await context.Reports.FindAsync(reportId);
    if (report == null)
    {
      logger.LogWarning("Report not found: {ReportId}", reportId);
      return;
    }

    try
    {
      logger.LogInformation("Processing report: {ReportId}", reportId);

      // Contact Service'ten tüm kişileri ve iletişim bilgilerini çek
      var contacts = await contactServiceClient.GetAllContactsAsync();

      // Konum bazlı istatistikleri hesapla
      var locationStats = contacts
          .SelectMany(c => c.ContactInfos.Where(ci => ci.InfoType == ContactInfoType.Location))
          .GroupBy(ci => ci.InfoContent)
          .Select(g => new LocationStatistic
          {
            Location = g.Key,
            PersonCount = g.Select(ci => ci.ContactId).Distinct().Count(),
            PhoneNumberCount = contacts
                  .Where(c => c.ContactInfos.Any(ci => ci.ContactId == c.Id && ci.InfoType == ContactInfoType.Location && ci.InfoContent == g.Key))
                  .SelectMany(c => c.ContactInfos.Where(ci => ci.InfoType == ContactInfoType.PhoneNumber))
                  .Count()
          })
          .ToList();

      report.Statistics = locationStats;
      report.Status = ReportStatus.Completed;
      report.CompletedDate = DateTime.UtcNow;

      await context.SaveChangesAsync();

      logger.LogInformation("Report completed: {ReportId}", reportId);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error processing report: {ReportId}", reportId);
      // Report durumunu error olarak işaretleyebiliriz
    }
  }
}

public record ReportRequestMessage(Guid ReportId);