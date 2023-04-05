using SchedulingTool.Api.Resources.projectdetail;

namespace SchedulingTool.Api.Resources;

public class ViewTaskResource
{
  public long TaskId { get; set; }
  public int Group { get; set; }
  public string TaskName { get; set; } = null!;
  public int Index { get; set; }
  public int NumberOfTeam { get; set; }
  public float Duration { get; set; }
  public float AmplifiedDuration { get; set; }
  public long GroupTaskId { get; set; }
  public string? Description { get; set; }
  public string? Note { get; set; }
  public ICollection<StepworkResource> Stepworks { get; set; } = null!;
}
