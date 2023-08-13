namespace SchedulingTool.Api.Resources.FormBody.Unsued;

public class PredecessorDetailFormData
{
  //public DataChange Change { get; set; }
  public long? StepworkId { get; set; }
  public int RelatedGroupTaskIndex { get; set; }
  public int RelatedTaskIndex { get; set; }
  public int RelatedStepworkIndex { get; set; }
  public decimal Lag { get; set; }
  public long Type { get; set; }
}
