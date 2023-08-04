namespace SchedulingTool.Api.Resources.Unused;

public class TaskDetailResource
{
  public long TaskId { get; set; }
  public string TaskName { get; set; } = null!;
  public int Index { get; set; }
  public int NumberOfTeam { get; set; }
  public float Duration { get; set; }
  public float AmplifiedDuration { get; set; }
  public long GroupTaskId { get; set; }
  public string? Description { get; set; }
  public string? Note { get; set; }
  public ICollection<StepworkDetailResource> Stepworks { get; set; } = null!;
}
