namespace SchedulingTool.Api.Domain.Models.Extended;

public class ProjectVersionDetails
{
  public long ProjectId { get; set; }
  public string ProjectName { get; set; } = null!;
  public DateTime ProjectModifiedDate { get; set; }
  public string ProjectModifiedBy { get; set; } = null!;
  public long VersionId { get; set; }
  public string VersionName { get; set; } = null!;
  public long UserId { get; set; }
  public DateTime CreatedDate { get; set; }
  public DateTime ModifiedDate { get; set; }
  public string ModifiedBy { get; set; } = null!;
  public bool IsActivated { get; set; }
  public int NumberOfMonths { get; set; }
  public bool IsShared { get; set; }
}
