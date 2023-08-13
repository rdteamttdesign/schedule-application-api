namespace SchedulingTool.Api.Domain.Models.Extended;

public class ExtendedPredecessor
{
  public string StepworkId { get; set; } = null!;
  public string RelatedStepworkId { get; set; } = null!;
  public long Type { get; set; }
  public decimal Lag { get; set; }
}
