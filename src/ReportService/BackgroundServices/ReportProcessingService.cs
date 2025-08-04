using ReportService.Services;
using Shared.Interfaces;
using System.Text.Json;

namespace ReportService.BackgroundServices;

public class ReportProcessingService(
    IServiceProvider serviceProvider,
    IKafkaConsumer kafkaConsumer,
    ILogger<ReportProcessingService> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Report processing service started");

    await kafkaConsumer.ConsumeAsync("report-requests", ProcessReportRequest, stoppingToken);
  }

  private async Task ProcessReportRequest(string message)
  {
    try
    {
      var reportRequest = JsonSerializer.Deserialize<ReportRequestMessage>(message);
      if (reportRequest == null)
      {
        logger.LogWarning("Invalid report request message: {Message}", message);
        return;
      }

      // Scoped service kullanımı
      using var scope = serviceProvider.CreateScope();
      var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

      await reportService.ProcessReportAsync(reportRequest.ReportId);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error processing report request: {Message}", message);
    }
  }
}