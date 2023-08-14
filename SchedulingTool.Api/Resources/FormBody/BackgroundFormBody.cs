using System.ComponentModel.DataAnnotations;

namespace SchedulingTool.Api.Resources.FormBody;

public class BackgroundFormBody
{
  [Required]
  public int Month { get; set; }

  [Required]
  public long? ColorId { get; set; }
}
