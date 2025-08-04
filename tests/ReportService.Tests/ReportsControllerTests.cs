using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ReportService.Controllers;
using ReportService.Services;
using Shared.Models;

namespace ReportService.Tests
{
  public class ReportsControllerTests
  {
    private readonly Mock<IReportService> _reportServiceMock = new();
    private readonly Mock<ILogger<ReportsController>> _loggerMock = new();
    private readonly ReportsController _controller;

    public ReportsControllerTests()
    {
      _controller = new ReportsController(_reportServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task RequestReport_ShouldReturnReportId_WhenSuccessful()
    {
      var expectedReportId = Guid.NewGuid();
      _reportServiceMock.Setup(x => x.RequestReportAsync())
          .ReturnsAsync(expectedReportId);

      var result = await _controller.RequestReport();

      var okResult = Assert.IsType<OkObjectResult>(result.Result);
      var json = JsonConvert.SerializeObject(okResult.Value);
      var response = JsonConvert.DeserializeObject<Dictionary<string, Guid>>(json);
      Assert.Equal(expectedReportId, response["ReportId"]);

      _loggerMock.Verify(x => x.Log(
          LogLevel.Information,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Report requested with ID: {expectedReportId}")),
          null,
          It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact]
    public async Task RequestReport_ShouldReturn500_WhenExceptionOccurs()
    {
      _reportServiceMock.Setup(x => x.RequestReportAsync())
          .ThrowsAsync(new Exception("Test exception"));

      var result = await _controller.RequestReport();

      var objectResult = Assert.IsType<ObjectResult>(result.Result);
      Assert.Equal(500, objectResult.StatusCode);
      Assert.Equal("An error occurred while requesting the report", objectResult.Value);

      _loggerMock.Verify(x => x.Log(
          LogLevel.Error,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error requesting report")),
          It.IsAny<Exception>(),
          It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }

    [Fact]
    public async Task GetReports_ShouldReturnReports_WhenSuccessful()
    {
      var expectedReports = new List<Report>
        {
            new Report { Id = Guid.NewGuid(), Status = ReportStatus.Completed },
            new Report { Id = Guid.NewGuid(), Status = ReportStatus.Preparing }
        };

      _reportServiceMock.Setup(x => x.GetReportsAsync())
          .ReturnsAsync(expectedReports);

      var result = await _controller.GetReports();

      var okResult = Assert.IsType<OkObjectResult>(result.Result);
      var reports = Assert.IsType<List<Report>>(okResult.Value);
      Assert.Equal(2, reports.Count);
    }

    [Fact]
    public async Task GetReport_ShouldReturnReport_WhenReportExists()
    {
      var expectedReportId = Guid.NewGuid();
      var expectedReport = new Report { Id = expectedReportId, Status = ReportStatus.Completed };

      _reportServiceMock.Setup(x => x.GetReportAsync(expectedReportId))
          .ReturnsAsync(expectedReport);

      var result = await _controller.GetReport(expectedReportId);

      var okResult = Assert.IsType<OkObjectResult>(result.Result);
      var report = Assert.IsType<Report>(okResult.Value);
      Assert.Equal(expectedReportId, report.Id);
    }

    [Fact]
    public async Task GetReport_ShouldReturnNotFound_WhenReportDoesNotExist()
    {
      var nonExistentId = Guid.NewGuid();
      _reportServiceMock.Setup(x => x.GetReportAsync(nonExistentId))
          .ReturnsAsync((Report)null);

      var result = await _controller.GetReport(nonExistentId);

      Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetReport_ShouldReturn500_WhenExceptionOccurs()
    {
      var testReportId = Guid.NewGuid();
      _reportServiceMock.Setup(x => x.GetReportAsync(testReportId))
          .ThrowsAsync(new Exception("Test exception"));

      var result = await _controller.GetReport(testReportId);

      var objectResult = Assert.IsType<ObjectResult>(result.Result);
      Assert.Equal(500, objectResult.StatusCode);
      Assert.Equal("An error occurred while retrieving the report", objectResult.Value);

      _loggerMock.Verify(x => x.Log(
          LogLevel.Error,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Error retrieving report {testReportId}")),
          It.IsAny<Exception>(),
          It.IsAny<Func<It.IsAnyType, Exception, string>>()),
          Times.Once);
    }
  }
}