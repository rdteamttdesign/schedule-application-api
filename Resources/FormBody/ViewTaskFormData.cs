using System.ComponentModel.DataAnnotations;

namespace SchedulingTool.Api.Resources.FormBody;

public class ViewTaskFormData
{
  [Required]
  public string Id { get; set; } = null!;
  public int Group { get; set; }
  public int DisplayOrder { get; set; }
}
