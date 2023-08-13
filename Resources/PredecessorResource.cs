namespace SchedulingTool.Api.Resources;

public class PredecessorResource
{
  public string Type { get; set; } = null!;
  public string Id { get; set; } = null!;
  public decimal LagDays { get; set; }
}
