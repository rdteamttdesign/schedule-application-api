using SchedulingTool.Api.Resources.Unused;

namespace SchedulingTool.Api.Resources;

public class ProjectSettingResource
{
  public long ProjectSettingId { get; set; }
  public long ProjectId { get; set; }
  public bool SeparateGroupTask { get; set; }
  public double AssemblyDurationRatio { get; set; }
  public double RemovalDurationRatio { get; set; }
  public ICollection<ColorDefResource> StepworkColors { get; set; } = null!;
  public ICollection<BackgroundColorResource> BackgroundColors { get; set; } = null!;
  public int NumberOfMonths { get; set; }
  public int ColumnWidth { get; set; }
  public double AmplifiedFactor { get; set; }
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
  public int Year { get; set; }
  public int Month { get; set; }
  public int Date { get; set; }
}
