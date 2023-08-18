using System.ComponentModel.DataAnnotations;

namespace SchedulingTool.Api.Resources.FormBody;

public class UpdateProjectFormData
{
  [Required]
  public string ProjectName { get; set; } = null!;
}
