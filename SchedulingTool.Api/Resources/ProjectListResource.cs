namespace SchedulingTool.Api.Resources;

public class ProjectListResource
{
  public long ProjectId { get; set; }
  public string ProjectName { get; set; } = null!;
  public bool IsShared { get; set; }
  public DateTime ModifiedDate { get; set; }
  public string ModifiedBy { get; set; } = null!;
  public IList<VersionResource> Versions { get; set; } = new List<VersionResource>();
}
