namespace SchedulingTool.Api.Resources.Unused;

public class VersionResource
{
  public long VersionId { get; set; }
  public string VersionName { get; set; } = null!;
  public long UserId { get; set; }
  public DateTime CreatedDate { get; set; }
  public DateTime ModifiedDate { get; set; }
  public bool IsActivated { get; set; }
  public int NumberOfMonths { get; set; }
}
