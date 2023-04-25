using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Extension;
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
            GroupTaskName = grouptaskFormData.Name!,
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
          TaskName = grouptaskFormData.Name ?? string.Empty,
          Index = grouptaskFormData.DisplayOrder,
          NumberOfTeam = grouptaskFormData.GroupsNumber,
          Duration = grouptaskFormData.Duration,
          GroupTaskLocalId = grouptaskFormData.GroupId,
          Description = grouptaskFormData.Detail,
          Note = grouptaskFormData.Note
        } );
        // case had stepwork
        if ( grouptaskFormData.Stepworks != null ) {

          if ( grouptaskFormData.Stepworks.Count > 1 && grouptaskFormData.GroupsNumber != 0 ) {
            var factor = setting!.AmplifiedFactor - 1;
            var firstStep = grouptaskFormData.Stepworks.ElementAt( 0 );
            firstStep.Start = firstStep.Start.ColumnWidthToDays( setting.ColumnWidth );
            firstStep.End = firstStep.PercentStepWork * grouptaskFormData.Duration / 100;
            if ( grouptaskFormData.GroupsNumber > 0 ) {
              firstStep.End *= setting!.AmplifiedFactor;
              firstStep.End /= grouptaskFormData.GroupsNumber > 0 ? grouptaskFormData.GroupsNumber : 1;
            }
            firstStep.End += firstStep.Start;

            var gap = firstStep.PercentStepWork * grouptaskFormData.Duration / 100 * factor;
            if ( grouptaskFormData.GroupsNumber > 0 )
              gap /= grouptaskFormData.GroupsNumber;

            for ( int i = 1; i < grouptaskFormData.Stepworks.Count; i++ ) {
              var stepwork = grouptaskFormData.Stepworks.ElementAt( i );
              stepwork.Start = stepwork.Start.ColumnWidthToDays( setting.ColumnWidth ) - gap;
              var stepDuration = stepwork.PercentStepWork * grouptaskFormData.Duration / 100;
              stepwork.End = stepDuration;
              if ( grouptaskFormData.GroupsNumber > 0 ) {
                stepwork.End *= setting!.AmplifiedFactor;
                stepwork.End /= grouptaskFormData.GroupsNumber > 0 ? grouptaskFormData.GroupsNumber : 1;
              }
              stepwork.End += stepwork.Start;

              if ( grouptaskFormData.GroupsNumber > 1 )
                gap += stepDuration * factor;
              else
                gap += stepDuration * factor / grouptaskFormData.GroupsNumber;
            }
          }
          else {
            foreach ( var stepworkFormData in grouptaskFormData.Stepworks ) {
              stepworkFormData.Start = stepworkFormData.Start.ColumnWidthToDays( setting.ColumnWidth );
              stepworkFormData.End = stepworkFormData.Start + stepworkFormData.PercentStepWork * grouptaskFormData.Duration / 100;
            }
          }

          foreach ( var stepworkFormData in grouptaskFormData.Stepworks ) {
            Stepworks.Add( new Stepwork()
            {
              LocalId = stepworkFormData.Id,
              Index = stepworkFormData.DisplayOrder,
              Portion = stepworkFormData.PercentStepWork / 100,
              TaskLocalId = grouptaskFormData.Id,
              ColorId = stepworkFormData.ColorId ?? 1,
              Duration = stepworkFormData.PercentStepWork * stepworkFormData.Duration / 100,
              Name = stepworkFormData.Name ?? string.Empty,
              Start = stepworkFormData.Start,
              End = stepworkFormData.End
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
          var _ = new Stepwork
          {
            LocalId = grouptaskFormData.Id,
            Index = grouptaskFormData.DisplayOrder,
            Portion = 100,
            TaskLocalId = grouptaskFormData.Id,
            ColorId = grouptaskFormData.ColorId ?? 1,
            Duration = grouptaskFormData.Duration,
            Name = grouptaskFormData.Name ?? string.Empty,
            Start = grouptaskFormData.Start.ColumnWidthToDays( setting.ColumnWidth )
          };
          _.End = _.Duration;
          if ( grouptaskFormData.GroupsNumber > 0 ) {
            _.End *= setting!.AmplifiedFactor;
            _.End /= grouptaskFormData.GroupsNumber > 0 ? grouptaskFormData.GroupsNumber : 1;
          }
          _.End += _.Start;
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
