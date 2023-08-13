namespace SchedulingTool.Api.Resources.FormBody;

public class CommonGroupTaskFormData
{
  public decimal Start { get; set; }
  public decimal Duration { get; set; }
  public string Name { set; get; } = null!;
  public string Id { set; get; } = null!;
  public string Type { set; get; } = null!;
  public bool? HideChildren { get; set; }
  public int DisplayOrder { get; set; }
  public long? ColorId { set; get; }
  public int GroupsNumber { set; get; }
  public string? Detail { get; set; } = null!;
  public string? GroupId { get; set; } = null!;
  public string? Note { get; set; } = null!;
  public ICollection<StepworkResource>? Stepworks { get; set; }
  public ICollection<PredecessorResource>? Predecessors { get; set; }
  public decimal End { get; set; }
}
