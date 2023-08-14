using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchedulingTool.Api.Resources.Extended;

public class GroupTaskDetailResource //: IEquatable<GroupTaskDetailResource>
{
  public long? GroupTaskId { get; set; }
  public string LocalId { get; set; } = null!;
  public string GroupTaskName { get; set; } = null!;
  public IList<TaskDetailResource> Tasks { get; set; } = null!;

  ////public bool Equals( GroupTaskDetailResource? other )
  //{
  //  if ( other == null )
  //    return false;

  //  if ( !GroupTaskName.Equals( other.GroupTaskName ) || Tasks.Count != other.Tasks.Count )
  //    return false;

  //  for ( int i = 0; i < Tasks.Count; i++ ) {
  //    if ( !Tasks [ i ].Equals( other.Tasks [ i ] ) ) {
  //      return false;
  //    }
  //  }

  //  return true;
  //}
}
