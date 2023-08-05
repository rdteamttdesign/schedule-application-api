using SchedulingTool.Api.Resources.Extended;

namespace SchedulingTool.Api.Resources.Unused;

public class ProjectDetailResource
{
  public ICollection<GroupTaskDetailResource> GroupTasks { get; set; } = null!;
  public ICollection<TaskDetailResource> Tasks { get; set; } = null!;
  public ICollection<StepworkDetailResource> Stepworks { get; set; } = null!;
}
