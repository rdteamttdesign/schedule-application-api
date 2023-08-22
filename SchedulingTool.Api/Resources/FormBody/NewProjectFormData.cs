using System.ComponentModel.DataAnnotations;

namespace SchedulingTool.Api.Resources.FormBody;

public class NewProjectFormData
{
  [Required]
  public string ProjectName { get; set; } = null!;

  [Required]
  public string VersionName { get; set; } = null!;

  [Required]
  [Range( 1, 100, ErrorMessage = "Please enter valid number of months (from 1 to 100)" )]
  public int NumberOfMonths { get; set; }

  [Required]
  public bool IsShared { get; set; }
}
