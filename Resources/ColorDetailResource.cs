using SchedulingTool.Api.Domain.Models.Enum;

namespace SchedulingTool.Api.Resources;

public class ColorDetailResource
{
  public long ColorId { get; set; }
  public string Name { get; set; } = null!;
  public string Code { get; set; } = null!;
  public ColorMode ColorMode { get; set; }
}
