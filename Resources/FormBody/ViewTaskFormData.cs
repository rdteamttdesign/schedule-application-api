using System.ComponentModel.DataAnnotations;

namespace SchedulingTool.Api.Resources.FormBody;

public class ViewTaskFormData
{
  [Required]
  public string Id { get; set; } = string.Empty;
  public int? Group { get; set; }
}
