namespace SchedulingTool.Api.Resources;

public class PredecessorDetailResource
{
  public long StepworkId { get; set; }
  public long RelatedStepworkId { get; set; }
  public long Type { get; set; }
  public double Lag { get; set; }
}
