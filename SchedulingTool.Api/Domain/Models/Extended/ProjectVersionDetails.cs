namespace SchedulingTool.Api.Domain.Models.Extended;

public class ProjectVersionDetails
{
  public long ProjectId { get; set; }
  public string ProjectName { get; set; } = null!;
  public DateTime ProjectModifiedDate { get; set; }
  public long VersionId { get; set; }
  public string VersionName { get; set; } = null!;
  public long UserId { get; set; }
  public DateTime CreatedDate { get; set; }
  public DateTime ModifiedDate { get; set; }
  public bool IsActivated { get; set; }
  public int NumberOfMonths { get; set; }
}
