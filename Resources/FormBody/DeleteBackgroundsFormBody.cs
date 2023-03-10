using System.ComponentModel.DataAnnotations;

namespace SchedulingTool.Api.Resources.FormBody;

public class DeleteBackgroundsFormBody
{
  [Required]
  public int DeleteFromMonth { get; set; }
}
