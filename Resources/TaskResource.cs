namespace SchedulingTool.Api.Resources;

public class TaskResource
{
  public decimal Start { get; set; }
  public decimal End { get; set; }
  public decimal Duration { get; set; }
  public string Name { get; set; } = null!;
  public string Id { get; set; } = null!;
  public string Type { get; set; } = null!;
  public string Detail { get; set; } = null!;
  public string GroupId { get; set; } = null!;
  public int DisplayOrder { get; set; }
  public string Note { get; set; } = null!;
  public long ColorId { get; set; }
  public int GroupsNumber { get; set; }
  public ICollection<StepworkResource>? Stepworks { get; set; }
  public ICollection<PredecessorResource>? Predecessors { get; set; }
}
