namespace SchedulingTool.Api.Resources;

public class ProjectSettingResource
{
  public long ProjectSettingId { get; set; }
  public long ProjectId { get; set; }
  public bool SeparateGroupTask { get; set; }
  public float AssemblyDurationRatio { get; set; }
  public float RemovalDurationRatio { get; set; }
  public IList<ColorDefResource> StepworkColors { get; set; } = null!;
  public IList<BackgroundColorResource> BackgroundColors { get; set; } = null!;
  public int NumberOfMonths { get; set; }
  public int ColumnWidth { get; set; }
  public float AmplifiedFactor { get; set; }
}

public class BackgroundColorResource
{
  public long ColorId { get; set; }
  public string Name { get; set; } = null!;
  public string Code { get; set; } = null!;
  public long Type { get; set; }
  public long ProjectId { get; set; }
  public string? DisplayMonths { get; set; }
  public ICollection<int> Months { get; set; } = null!;
}

public class ProjectBackgroundResource
{
  public long? ColorId { get; set; }
  public string Name { get; set; } = null!;
  public string ColorCode { get; set; } = null!;
  public long Type { get; set; }
  public int Month { get; set; }
}
