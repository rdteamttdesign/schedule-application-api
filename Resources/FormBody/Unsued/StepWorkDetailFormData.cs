namespace SchedulingTool.Api.Resources.FormBody.Unsued;

public class StepworkDetailFormData
{
  //public DataChange Change { get; set; }
  //public long TaskId { get; set; }
  public decimal Portion { get; set; }
  public long ColorId { get; set; }
  public int Index { get; set; }
  //public int TaskIndex { get; set; }
  //public long GroupTaskIndex { get; set; }
  public ICollection<PredecessorDetailFormData> Predecessors { get; set; } = null!;
}
