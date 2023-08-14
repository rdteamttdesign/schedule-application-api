using System.ComponentModel.DataAnnotations;

namespace SchedulingTool.Api.Resources.FormBody;

public class UpdateVersionFormData
{
  [Required]
  public string VersionName { get; set; } = null!;
}
