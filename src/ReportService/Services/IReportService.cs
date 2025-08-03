using Shared.Models;

namespace ReportService.Services;

public interface IReportService
{
  Task<Guid> RequestReportAsync();
  Task<List<Report>> GetReportsAsync();
  Task<Report?> GetReportAsync(Guid id);
  Task ProcessReportAsync(Guid reportId);
}