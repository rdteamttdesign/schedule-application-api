using SchedulingTool.Api.Domain.Models.Enum;

namespace SchedulingTool.Api.Resources.FormBody;

public class GroupTaskDetailFormData
{
  //public DataChange Change { get; set; }
  public long GroupTaskId { get; set; }
  public string GroupTaskName { get; set; } = null!;
  public int Index { get; set; }
  //public ICollection<TaskDetailFormData> Tasks { get; set; } = null!;
}
