namespace SchedulingTool.Api.Resources;

public class ProjectListResource
{
  public long ProjectId { get; set; }
  public string ProjectName { get; set; } = null!;
  public DateTime ModifiedDate { get; set; } 
  public IList<VersionResource> Versions { get; set; } = new List<VersionResource>();
}
