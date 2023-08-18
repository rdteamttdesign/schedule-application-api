using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulingTool.Api.Domain.Models;

public class ViewTaskDetail
{
  public long TaskId { get; set; }
  public string TaskLocalId { get; set; } = null!;
  public int Group { get; set; }
  public string TaskName { get; set; } = null!;
  public int Index { get; set; }
  public int NumberOfTeam { get; set; }
  public double Duration { get; set; }

  [NotMapped]
  public double MinStart { get; set; }

  [NotMapped]
  public double MaxEnd { get; set; }

  public double AmplifiedDuration { get; set; }
  public long GroupTaskId { get; set; }
  public string GroupTaskLocalId { get; set; } = null!;
  public string GroupTaskName { get; set; } = null!;
  public string? Description { get; set; }
  public string? Note { get; set; }
  public int DisplayOrder { get; set; }
  public bool IsHidden { get; set; }
  public bool IsDayFormat { get; set; }
  public ICollection<Stepwork> Stepworks { get; set; } = null!;
}
