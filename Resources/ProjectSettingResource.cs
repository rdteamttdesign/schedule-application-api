namespace SchedulingTool.Api.Resources;

public class ProjectSettingResource
{
  public long ProjectSettingId { get; set; }
  public long VersionId { get; set; }
  public bool SeparateGroupTask { get; set; }
  public float AssemblyDurationRatio { get; set; }
  public float RemovalDurationRatio { get; set; }
  public ICollection<ColorDefResource> StepworkColors { get; set; } = null!;
  public ICollection<BackgroundColorResource> BackgroundColors { get; set; } = null!;
  public int NumberOfMonths { get; set; }
  public int ColumnWidth { get; set; }
  public float AmplifiedFactor { get; set; }
  public bool IncludeYear { get; set; }
  public int StartYear { get; set; }
  public int StartMonth { get; set; }
}

public class BackgroundColorResource
{
  public long ColorId { get; set; }
  public string Name { get; set; } = null!;
  public string Code { get; set; } = null!;
  public long Type { get; set; }
  public long VersionId { get; set; }
  public ICollection<BackgroundDateRange> DateRanges { get; set; } = new List<BackgroundDateRange>();
}

public class BackgroundDateRange
{
  public string DisplayText { get; set; } = null!;
  public int StartYear { get; set; }
  public int StartMonth { get; set; }
  public int StartDate { get; set; }
  public int ToYear { get; set; }
  public int ToMonth { get; set; }
  public int ToDate { get; set; }
}
