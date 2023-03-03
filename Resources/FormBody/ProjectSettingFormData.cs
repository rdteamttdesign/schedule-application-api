using System.ComponentModel.DataAnnotations;

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
}
