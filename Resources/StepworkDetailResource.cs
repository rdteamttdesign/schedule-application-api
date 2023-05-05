namespace SchedulingTool.Api.Resources;

public class StepworkDetailResource
{
  public long StepworkId { get; set; }
  public int Index { get; set; }
  public float Portion { get; set; }
  public long TaskId { get; set; }
  public long ColorId { get; set; }
  public string ColorCode { get; set; } = null!;

  public ICollection<PredecessorDetailResource> Predecessors { get; set; } = null!;
}
