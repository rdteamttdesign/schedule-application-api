using SchedulingTool.Api.Resources.projectdetail;

namespace SchedulingTool.Api.Resources;

public class ViewTaskResource
{
  public string Id { get; set; } = null!;
  public int Group { get; set; }
  public int DisplayOrder { get; set; }
}
