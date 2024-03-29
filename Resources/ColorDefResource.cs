﻿namespace SchedulingTool.Api.Resources;

public class ColorDefResource
{
  public long ColorId { get; set; }
  public string Name { get; set; } = null!;
  public string Code { get; set; } = null!;
  public long Type { get; set; }
  public long ProjectId { get; set; }
  public ulong IsDefault { get; set; }
}
