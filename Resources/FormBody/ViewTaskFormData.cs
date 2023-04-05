using System.ComponentModel.DataAnnotations;

namespace SchedulingTool.Api.Resources.FormBody;

public class ViewTaskFormData
{
  [Required]
  public string TaskId { get; set; } = null!;
  public int Group { get; set; }
}
