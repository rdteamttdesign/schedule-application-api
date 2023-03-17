﻿using System.ComponentModel.DataAnnotations;

namespace SchedulingTool.Api.Resources.FormBody;

public class ProjectSettingFormData
{
  [Required]
  public bool SeparateGroupTask { get; set; }


  [Required]
  [Range( 0.09, 1, ErrorMessage = "Please enter valid ratio (from 0.01 to 1)" )]
  public float AssemblyDurationRatio { get; set; }


  [Required]
  [Range( 0.09, 1, ErrorMessage = "Please enter valid ratio (from 0.01 to 1)" )]
  public float RemovalDurationRatio { get; set; }

  [Required]
  public ICollection<ColorDefFormData> StepworkColors { get; set; } = null!;

  [Required]
  public ICollection<BackgroundColorFormData> BackgroundColors { get; set; } = null!;

  [Required]
  public int NumberOfMonths { get; set; }
}

public class ColorDefFormData
{
  public long? ColorId { get; set; }
  public string Name { get; set; } = null!;
  public string Code { get; set; } = null!;
  public long Type { get; set; }
  public long ProjectId { get; set; }
  public bool IsDefault { get; set; }
}

public class BackgroundColorFormData
{
  public long? ColorId { get; set; }
  public string Name { get; set; } = null!;
  public string Code { get; set; } = null!;
  public long Type { get; set; }
  public long ProjectId { get; set; }
  public ICollection<int> Months { get; set; } = null!;
}