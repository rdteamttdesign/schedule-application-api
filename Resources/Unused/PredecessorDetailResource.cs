namespace SchedulingTool.Api.Resources.Unused;

public class PredecessorDetailResource
{
  public long StepworkId { get; set; }
  public long RelatedStepworkId { get; set; }
  public long Type { get; set; }
  public decimal Lag { get; set; }
}
