namespace SchedulingTool.Api.Resources.Unused;

public class StepworkResource
{
  public float Start { get; set; }
  public float Duration { get; set; }
  public float PercentStepWork { get; set; }
  public string? Name { get; set; }
  public string ParentTaskId { get; set; } = null!;
  public string Id { get; set; } = null!;
  public string Type { get; set; } = null!;
  public string GroupId { get; set; } = null!;
  public int DisplayOrder { get; set; }
  public ICollection<PredecessorResource>? Predecessors { get; set; }
  public long? ColorId { get; set; }
  public float End { get; set; }
  public int GroupNumbers { get; set; }
}