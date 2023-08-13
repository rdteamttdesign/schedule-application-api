namespace SchedulingTool.Api.Resources;

public class StepworkResource
{
  public decimal Start { get; set; }
  public decimal Duration { get; set; }
  public decimal PercentStepWork { get; set; }
  public string? Name { get; set; }
  public string ParentTaskId { get; set; } = null!;
  public string Id { get; set; } = null!;
  public string Type { get; set; } = null!;
  public string GroupId { get; set; } = null!;
  public int DisplayOrder { get; set; }
  public ICollection<PredecessorResource>? Predecessors { get; set; }
  public long? ColorId { get; set; }
  public decimal End { get; set; }
  public int GroupNumbers { get; set; }
}