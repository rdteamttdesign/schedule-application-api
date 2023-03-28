using System.ComponentModel.DataAnnotations;

namespace SchedulingTool.Api.Resources.FormBody;

public class ColorFormData
{
  [Required]
  public string Name { get; set; } = null!;

  [Required]
  //[RegularExpression( "^#(?:[0-9a-fA-F]{3}){1,2}$", ErrorMessage = "Invalid color code." )]
  public string Code { get; set; } = null!;

  [Required]
  public long Type { get; set; }
}
