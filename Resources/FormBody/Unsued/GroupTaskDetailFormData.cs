namespace SchedulingTool.Api.Resources.FormBody.Unsued;

public class GroupTaskDetailFormData
{
  //public DataChange Change { get; set; }
  public string GroupTaskName { get; set; } = null!;
  public int Index { get; set; }
  public ICollection<TaskDetailFormData> Tasks { get; set; } = null!;
}
