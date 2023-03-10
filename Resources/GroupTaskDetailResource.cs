namespace SchedulingTool.Api.Resources;

public class GroupTaskDetailResource
{
  public long GroupTaskId { get; set; }
  public string GroupTaskName { get; set; } = null!;
  public long ProjectId { get; set; }
  public int Index { get; set; }
  public ICollection<TaskDetailResource> Tasks { get; set; } = null!;
}
