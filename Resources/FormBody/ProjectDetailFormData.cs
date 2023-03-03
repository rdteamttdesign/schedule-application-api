namespace SchedulingTool.Api.Resources.FormBody;

public class ProjectDetailFormData
{
  //public long ProjectId { get; set; }
  public ICollection<GroupTaskDetailFormData> GroupTasks { get; set; } = null!;
  //public ICollection<TaskDetailFormData> Tasks { get; set; } = null!;
  //public ICollection<StepworkDetailFormData> Stepworks { get; set; } = null!;
  //public ICollection<PredecessorDetailFormData> Predecessors { get; set; } = null!;
}
