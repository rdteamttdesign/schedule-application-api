using SchedulingTool.Api.Domain.Models.Enum;

namespace SchedulingTool.Api.Resources.FormBody;

public class StepworkDetailFormData
{
  //public DataChange Change { get; set; }
  //public long TaskId { get; set; }
  public long StepworkId { get; set; }
  public float Portion { get; set; }
  public long ColorId { get; set; }
  public int Index { get; set; }
  public int TaskIndex { get; set; }
  public long GroupTaskIndex { get; set; }
  public ICollection<PredecessorDetailFormData> Predecessors { get; set; } = null!;
}
