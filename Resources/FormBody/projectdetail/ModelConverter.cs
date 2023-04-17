using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Extension;
using System.Dynamic;
using Task = SchedulingTool.Api.Domain.Models.Task;

namespace SchedulingTool.Api.Resources.FormBody.projectdetail;

public class ModelConverter
{
  public List<GroupTask> GroupTasks { get; private set; } = new List<GroupTask>();
  public List<Task> Tasks { get; private set; } = new List<Task>();
  public List<Stepwork> Stepworks { get; private set; } = new List<Stepwork>();
  public List<ExtendedPredecessor> Predecessors { get; private set; } = new List<ExtendedPredecessor>();

  public ModelConverter( 
    long projectId, 
    ProjectSetting setting,
    ICollection<GroupTaskFormData> grouptaskFormDataList )
  {
    foreach ( GroupTaskFormData grouptaskFormData in grouptaskFormDataList ) {
      // case type project ~ group task
      if ( grouptaskFormData.Type == "project" ) {
        GroupTasks.Add(
          new GroupTask()
          {
            LocalId = grouptaskFormData.Id,
            GroupTaskName = grouptaskFormData.Name,
            ProjectId = projectId,
            Index = grouptaskFormData.DisplayOrder,
            HideChidren = grouptaskFormData.HideChildren ?? false
          } );
      }
      // case type task
      else if ( grouptaskFormData.Type == "task" ) {
        if ( grouptaskFormData.GroupId == null ) {
          continue;
        }
        Tasks.Add( new Task()
        {
          LocalId = grouptaskFormData.Id,
          TaskName = grouptaskFormData.Name,
          Index = grouptaskFormData.DisplayOrder,
          NumberOfTeam = grouptaskFormData.GroupsNumber,
          Duration = grouptaskFormData.Duration / setting.AmplifiedFactor,
          GroupTaskLocalId = grouptaskFormData.GroupId,
          Description = grouptaskFormData.Detail,
          Note = grouptaskFormData.Note
        } );
        // case had stepwork
        if ( grouptaskFormData.Stepworks != null ) {
          foreach ( var stepworkFormData in grouptaskFormData.Stepworks ) {
            Stepworks.Add( new Stepwork()
            {
              LocalId = stepworkFormData.Id,
              Index = stepworkFormData.DisplayOrder,
              Portion = stepworkFormData.PercentStepWork / 100,
              TaskLocalId = grouptaskFormData.Id,
              ColorId = 1, //stepworkFormData.ColorId ?? 1,
              Duration = stepworkFormData.PercentStepWork * stepworkFormData.Duration / 100,
              Name = stepworkFormData.Name ?? string.Empty,
              Start = stepworkFormData.Start.ColumnWidthToDays( setting.ColumnWidth ),
              End = stepworkFormData.End.ColumnWidthToDays( setting.ColumnWidth )
            } );
            if ( stepworkFormData.Predecessors == null ) {
              continue;
            }
            // case had prodecessor
            foreach ( var predecessorFormData in stepworkFormData.Predecessors ) {
              Predecessors.Add( new ExtendedPredecessor()
              {
                StepworkId = stepworkFormData.Id,
                RelatedStepworkId = predecessorFormData.Id,
                Type = predecessorFormData.Type == "FS" ? 1 : ( predecessorFormData.Type == "SS" ? 2 : 3 ),
                Lag = predecessorFormData.LagDays
              } );
            }
          }
        }
        // case not stepwork
        if ( grouptaskFormData.Stepworks == null || grouptaskFormData.Stepworks.Count == 0 ) {
          var _ = new Stepwork()
          {
            LocalId = grouptaskFormData.Id,
            Index = grouptaskFormData.DisplayOrder,
            Portion = 100,
            TaskLocalId = grouptaskFormData.Id,
            ColorId = 1, // grouptaskFormData.ColorId ?? 1,
            Duration = grouptaskFormData.Duration / setting.AmplifiedFactor,
            Name = grouptaskFormData.Name ?? string.Empty,
            Start = grouptaskFormData.Start.ColumnWidthToDays( setting.ColumnWidth ),
            End = grouptaskFormData.End.ColumnWidthToDays( setting.ColumnWidth )
          };
          Stepworks.Add( _ );
        }
        // case not stepwork but had predecessor
        if ( grouptaskFormData.Predecessors != null && grouptaskFormData.Stepworks == null ) {
          foreach ( var predecessorFormData in grouptaskFormData.Predecessors ) {
            Predecessors.Add( new ExtendedPredecessor()
            {
              StepworkId = grouptaskFormData.Id,
              RelatedStepworkId = predecessorFormData.Id,
              Type = predecessorFormData.Type == "FS" ? 1 : ( predecessorFormData.Type == "SS" ? 2 : 3 ),
              Lag = predecessorFormData.LagDays
            } );
          }
        }
      }
    }
  }
}

public class ExtendedPredecessor
{
  public string StepworkId { get; set; } = null!;
  public string RelatedStepworkId { get; set; } = null!;
  public long Type { get; set; }
  public float Lag { get; set; }
}
