using SchedulingTool.Api.Domain.Models.Enum;

namespace SchedulingTool.Api.Resources.FormBody;

public class TaskDetailFormData
{
  //public DataChange Change { get; set; }
  //public long GroupTaskId { get; set; }
  public long TaskId { get; set; }
  public string TaskName { get; set; } = null!;
  public int Index { get; set; }
  public int GroupIndex { get; set; }
  public int NumberOfTeam { get; set; }
  public float Duration { get; set; }
  public float AmplifiedDuration { get; set; }
  public string? Description { get; set; }
  public string? Note { get; set; }
  //public ICollection<StepworkDetailFormData> Stepworks { get; set; } = null!;
}
