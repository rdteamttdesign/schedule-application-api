namespace SchedulingTool.Api.Resources.FormBody;

public class ViewTaskDetailFormData
{
  public string Id { get; set; } = null!;
  public bool IsHidden { get; set; }
  public string? Name { get; set; }
  public string? Detail { get; set; }
  public string? Note { get; set; }
  public string Type { get; set; } = null!;
  public bool IsDayFormat { get; set; }
}
