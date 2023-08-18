namespace SchedulingTool.Api.Resources;

public class StepworkDetailResource
{
  public long StepworkId { get; set; }
  public int Index { get; set; }
  public double Start { get; set; }
  public double Portion { get; set; }
  public long TaskId { get; set; }
  public long ColorId { get; set; }
  public ColorDetailResource ColorDetail { get; set; } = null!;
  public bool IsSubStepwork { get; set; }

  public ICollection<PredecessorDetailResource> Predecessors { get; set; } = null!;
}
