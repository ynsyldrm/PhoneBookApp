namespace Shared.Models
{
  public class Report
  {
    public Guid Id { get; set; }
    public DateTime RequestedDate { get; set; }
    public ReportStatus Status { get; set; }
    public List<LocationStatistic> Statistics { get; set; } = new();
  }

  public class LocationStatistic
  {
    public string Location { get; set; }
    public int PersonCount { get; set; }
    public int PhoneNumberCount { get; set; }
  }

  public enum ReportStatus
  {
    Preparing,
    Completed
  }
}
