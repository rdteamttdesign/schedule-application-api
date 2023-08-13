using DocumentFormat.OpenXml.Office2021.DocumentTasks;

namespace SchedulingTool.Api.Resources.Extended;

public class TaskDetailResource : IEquatable<TaskDetailResource>
{
  public long? TaskId { get; set; }
  public string LocalId { get; set; } = null!;
  public string TaskName { get; set; } = null!;
  public int NumberOfTeam { get; set; }
  public decimal Duration { get; set; }
  public string? Description { get; set; }
  public string? Note { get; set; }
  public IList<StepworkDetailResource> Stepworks { get; set; } = null!;

  public bool Equals( TaskDetailResource? other )
  {
    if ( other == null )
      return false;

    if ( !TaskName.Equals( other.TaskName )
      || !NumberOfTeam.Equals( other.NumberOfTeam )
      || Math.Round( Duration, 6 ) != Math.Round( other.Duration, 6 )
      || Description != other.Description
      || Note != other.Note
      || Stepworks.Count != other.Stepworks.Count )
      return false;

    for ( int i = 0; i < Stepworks.Count; i++ ) {
      if ( !Stepworks [ i ].Equals( other.Stepworks [ i ] ) ) {
        return false;
      }
    }

    return true;
  }
}
