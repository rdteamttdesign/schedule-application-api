namespace SchedulingTool.Api.Resources;

public class UpdateGrouptaskResource
{
  public string Change { get; set; } = null!;
  public string Id { get; set; } = null!;
  public string? Name { get; set; }
  public IList<UpdateTaskResource>? Tasks { get; set; }
}

public class UpdateTaskResource
{
  public string Change { get; set; } = null!;
  public string Id { get; set; } = null!;
  public string? Name { get; set; }
  public float Duration { get; set; }
  public int GroupsNumber { get; set; }
  public string? Note { get; set; }
  public string? Detail { get; set; }
  public IList<UpdateStepworkResource>? Stepworks { get; set; }
}

public class UpdateStepworkResource
{
  public string Id { get; set; } = null!;
  public long ColorId { get; set; }
  public float PercentStepWork { get; set; }
}