namespace SchedulingTool.Api.Resources.FormBody;

public class ViewFormData
{
  public string ViewName { get; set; } = null!;
  public ICollection<ViewTaskFormData> Tasks { get; set; } = null!;
}
