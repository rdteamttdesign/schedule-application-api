using SchedulingTool.Api.Domain.Models.Enum;

namespace SchedulingTool.Api.Resources.FormBody;

public class PredecessorDetailFormData
{
  //public DataChange Change { get; set; }
  //public long StepworkId { get; set; }
  public int RelatedGroupIndex { get; set; }
  public int RelatedTaskIndex { get; set; }
  public int RelatedStepworkIndex { get; set; }
  public float Lag { get; set; }
  public long Type { get; set; }
}
