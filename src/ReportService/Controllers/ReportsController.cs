using Microsoft.AspNetCore.Mvc;
using ReportService.Services;
using Shared.Models;

namespace ReportService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(IReportService reportService, ILogger<ReportsController> logger) : ControllerBase
{
  [HttpPost("request")]
  public async Task<ActionResult<Guid>> RequestReport()
  {
    try
    {
      var reportId = await reportService.RequestReportAsync();
      logger.LogInformation("Report requested with ID: {ReportId}", reportId);
      return Ok(new { ReportId = reportId });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error requesting report");
      return StatusCode(500, "An error occurred while requesting the report");
    }
  }

  [HttpGet]
  public async Task<ActionResult<List<Report>>> GetReports()
  {
    try
    {
      var reports = await reportService.GetReportsAsync();
      return Ok(reports);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error retrieving reports");
      return StatusCode(500, "An error occurred while retrieving reports");
    }
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<Report>> GetReport(Guid id)
  {
    try
    {
      var report = await reportService.GetReportAsync(id);
      if (report == null)
      {
        return NotFound();
      }

      return Ok(report);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error retrieving report {ReportId}", id);
      return StatusCode(500, "An error occurred while retrieving the report");
    }
  }
}