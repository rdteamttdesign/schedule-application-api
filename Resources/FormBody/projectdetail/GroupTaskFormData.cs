using SchedulingTool.Api.Resources.projectdetail;

namespace SchedulingTool.Api.Resources.FormBody.projectdetail;

public class GroupTaskFormData
{
  public int Start { get; set; }
  public float Duration { get; set; }
  public string Name { set; get; } = null!;
  public string Id { set; get; } = null!;
  public string Type { set; get; } = null!;
  public bool? HideChildren { get; set; }
  public int DisplayOrder { get; set; }
  public long ColorId { set; get; }
  public int GroupsNumber { set; get; }
  public string? Detail { get; set; } = null!;
  public string? GroupId { get; set; } = null!;
  public string? Note { get; set; } = null!;
  public ICollection<StepworkResource>? Stepworks { get; set; }
  public ICollection<PredecessorResource>? Predecessors { get; set; }
}

public class StepworkFormData
{
  public int Start { get; set; }
  public float Duration { get; set; }
  public float PercentStepWork { get; set; }
  public string Name { get; set; } = null!;
  public string ParentTaskId { get; set; } = null!;
  public string Id { get; set; } = null!;
  public string Type { get; set; } = null!;
  public string GroupId { get; set; } = null!;
  public int DisplayOrder { get; set; }
  public ICollection<PredecessorFormData> Predecessors { get; set; } = null!;
  public long ColorId { get; set; }
}

public class PredecessorFormData
{
  public string Type { get; set; } = null!;
  public string Id { get; set; } = null!;
  public float LagDays { get; set; }
}
