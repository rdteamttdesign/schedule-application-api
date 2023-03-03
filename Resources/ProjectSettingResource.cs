namespace SchedulingTool.Api.Resources;

public class ProjectSettingResource
{
  public long ProjectSettingId { get; set; }
  public long ProjectId { get; set; }
  public bool SeparateGroupTask { get; set; }
  public float AssemblyDurationRatio { get; set; }
  public float RemovalDurationRatio { get; set; }
}
