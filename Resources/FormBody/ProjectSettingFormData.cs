using System.ComponentModel.DataAnnotations;

namespace SchedulingTool.Api.Resources.FormBody;

public class ProjectSettingFormData
{
  [Required]
  public bool SeparateGroupTask { get; set; }


  [Required]
  [Range( 0.09, 1, ErrorMessage = "Please enter valid ratio (from 0.01 to 1)" )]
  public decimal AssemblyDurationRatio { get; set; }


  [Required]
  [Range( 0.09, 1, ErrorMessage = "Please enter valid ratio (from 0.01 to 1)" )]
  public decimal RemovalDurationRatio { get; set; }

  [Required]
  public ICollection<ColorDefFormData> StepworkColors { get; set; } = null!;

  [Required]
  public ICollection<BackgroundColorFormData> BackgroundColors { get; set; } = null!;

  [Required]
  public int NumberOfMonths { get; set; }

  [Required]
  public int ColumnWidth { get; set; }

  [Required]
  public bool IncludeYear { get; set; }

  [Required]
  public int StartYear { get; set; }

  [Required]
  public int StartMonth { get; set; }

  [Required]
  public decimal AmplifiedFactor { get; set; }
}

public class ColorDefFormData
{
  public long? ColorId { get; set; }
  public string Name { get; set; } = null!;
  public string Code { get; set; } = null!;
  public long Type { get; set; }
  public bool IsDefault { get; set; }
}

public class BackgroundColorFormData
{
  public long? ColorId { get; set; }
  public string Name { get; set; } = null!;
  public string Code { get; set; } = null!;
  public long Type { get; set; }
  public IList<string> DisplayDateRanges { get; set; } = null!;
}