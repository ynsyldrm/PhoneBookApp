using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class Report
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
  public ReportStatus Status { get; set; } = ReportStatus.Preparing;
  public List<LocationStatistic> Statistics { get; set; } = [];
  public DateTime? CompletedDate { get; set; }
}

public class LocationStatistic
{
  [Required]
  public required string Location { get; set; }
  public int PersonCount { get; set; }
  public int PhoneNumberCount { get; set; }
}

public enum ReportStatus
{
  Preparing,
  Completed
}