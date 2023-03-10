using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Resources;

public class StepworkDetailResource
{
  public long StepworkId { get; set; }
  public int Index { get; set; }
  public float Duration { get; set; }
  public long TaskId { get; set; }
  public long ColorId { get; set; }

  public ICollection<PredecessorDetailResource> Predecessors { get; set; } = null!;
}
