using SchedulingTool.Api.Domain.Models.Enum;

namespace SchedulingTool.Api.Resources.FormBody;

public class PredecessorDetailFormData
{
  public DataChange Change { get; set; }
  //public long StepworkId { get; set; }
  public long RelatedStepworkId { get; set; }
  public float Lag { get; set; }
  public long Type { get; set; }
}
