namespace SchedulingTool.Api.Resources;

public class ViewResource
{
  public long ViewId { get; set; }
  public string ViewName { get; set; } = null!;
  public long ProjectId { get; set; }

  public ICollection<ViewTaskResource> ViewTasks { get; set; } = new List<ViewTaskResource>();
}
