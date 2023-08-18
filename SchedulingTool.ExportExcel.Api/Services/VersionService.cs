using AutoMapper;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Models.Enum;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.ExportExcel;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.Extended;

namespace SchedulingTool.Api.Services;

public class VersionService : IVersionService
{
  private readonly IProjectRepository _projectRepository;
  private readonly IProjectVersionRepository _projectVersionRepository;
  private readonly IProjectSettingRepository _projectSettingRepository;
  private readonly IColorDefRepository _colorRepository;
  private readonly IBackgroundRepository _backgroundRepository;
  private readonly IGroupTaskRepository _groupTaskRepository;
  private readonly ITaskRepository _taskRepository;
  private readonly IStepworkRepository _stepworkRepository;
  private readonly IPredecessorRepository _predecessorRepository;
  private readonly IViewRepository _viewRepository;
  private readonly IMapper _mapper;

  public VersionService(
    IProjectRepository projectRepository,
    IProjectVersionRepository projectVersionRepository,
    IProjectSettingRepository projectSettingRepository,
    IColorDefRepository colorRepository,
    IBackgroundRepository backgroundRepository,
    IGroupTaskRepository groupTaskRepository,
    ITaskRepository taskRepository,
    IStepworkRepository stepworkRepository,
    IPredecessorRepository predecessorRepository,
    IViewRepository viewRepository,
    IMapper mapper )
  {
    _projectRepository = projectRepository;
    _projectVersionRepository = projectVersionRepository;
    _projectSettingRepository = projectSettingRepository;
    _colorRepository = colorRepository;
    _backgroundRepository = backgroundRepository;
    _groupTaskRepository = groupTaskRepository;
    _taskRepository = taskRepository;
    _stepworkRepository = stepworkRepository;
    _predecessorRepository = predecessorRepository;
    _viewRepository = viewRepository;
    _mapper = mapper;
  }

  private async Task<string?> GetProjectNameOfVersion( long versionId )
  {
    var projectVersion = await _projectVersionRepository.GetProjectVersionByVersionId( versionId );
    if ( projectVersion == null ) {
      return null;
    }
    var project = await _projectRepository.GetById( projectVersion.ProjectId );
    return project?.ProjectName;
  }

  public async Task<ServiceResponse<ExportExcelResource>> ExportToExcel( long versionId )
  {
    var projectName = await GetProjectNameOfVersion( versionId );
    if ( projectName == null ) {
      return new ServiceResponse<ExportExcelResource>( "Project not found." );
    }
    var groupTaskResources = await GetGroupTaskResourcesByProjectId( versionId );
    var bgResources = await GetBackgrounds( versionId );
    var usedColor = await GetUsedColor( versionId, groupTaskResources, bgResources );
    var setting = await _projectSettingRepository.GetByVersionId( versionId );
    if ( setting == null ) {
      return new ServiceResponse<ExportExcelResource>( "Project setting not found." );
    }
    var viewResources = await GetViewTasks( versionId );

    var projectResource = new ProjectResource()
    {
      ProjectName = projectName,
      Backgrounds = bgResources,
      UsedColors = usedColor,
      Setting = setting,
      Grouptasks = groupTaskResources,
      ChartStepworks = GrouptaskResourcesToChartStepworks( groupTaskResources, setting.AmplifiedFactor, setting.IncludeYear ),
      ViewTasks = viewResources
    };

    var success = ExportExcel.ExportExcel.GetFile( projectResource, out var result );

    return success
      ? new ServiceResponse<ExportExcelResource>( new ExportExcelResource() { FilePath = result } )
      : new ServiceResponse<ExportExcelResource>( result );
  }

  private async Task<IList<ColorDef>> GetUsedColor( long versiontId, IEnumerable<GroupTaskDetailResource> tasks, IEnumerable<ProjectBackgroundResource> background )
  {
    var colorSetting = await _colorRepository.GeAllColorsByVersionId( versiontId );
    var usedStepworkColorIds = tasks.SelectMany( x => x.Tasks ).SelectMany( x => x.Stepworks ).Select( x => x.ColorId ).Distinct();
    var usedBgColorIds = background.Select( x => x.ColorId );

    return colorSetting.Where( x => usedStepworkColorIds.Contains( x.ColorId ) || usedBgColorIds.Contains( x.ColorId ) ).ToList();
  }

  private async Task<Dictionary<View, List<ViewTaskDetail>>> GetViewTasks( long versionId )
  {
    var result = new Dictionary<View, List<ViewTaskDetail>>();
    var views = await _viewRepository.GetViewsByVersionId( versionId );
    foreach ( var view in views ) {
      var viewTasks = await GetViewTasks( versionId, view.ViewId );
      result.Add( view, viewTasks.ToList() );
    }
    return result;
  }

  private async Task<List<GroupTaskDetailResource>> GetGroupTaskResourcesByProjectId( long versionId )
  {
    var groupTasks = await _groupTaskRepository.GetGroupTasksByVersionId( versionId );
    var groupTaskResources = _mapper.Map<List<GroupTaskDetailResource>>( groupTasks );

    var stepworkColors = ( await _colorRepository.GetStepworkColorsByProjectId( versionId ) )
      .ToDictionary( x => x.ColorId, x =>
      new ColorDetailResource()
      {
        Name = x.Name,
        Code = x.Code,
        ColorId = x.ColorId,
        ColorMode = x.IsInstall == 0 ? ColorMode.Install : ( x.IsInstall == 1 ? ColorMode.Removal : ColorMode.Custom )
      } );

    foreach ( var groupTaskResource in groupTaskResources ) {
      var tasks = await _taskRepository.GetTasksByGroupTaskId( groupTaskResource.GroupTaskId );
      groupTaskResource.Tasks = _mapper.Map<List<TaskDetailResource>>( tasks );
      foreach ( var taskResource in groupTaskResource.Tasks ) {
        var stepworks = await _stepworkRepository.GetStepworksByTaskId( taskResource.TaskId );
        taskResource.Stepworks = _mapper.Map<List<StepworkDetailResource>>( stepworks );
        foreach ( var stepworkResource in taskResource.Stepworks ) {
          var predecessor = await _predecessorRepository.GetPredecessorsByStepworkId( stepworkResource.StepworkId );
          stepworkResource.Predecessors = _mapper.Map<List<PredecessorDetailResource>>( predecessor );
          if ( stepworkColors.ContainsKey( stepworkResource.ColorId ) ) {
            stepworkResource.ColorDetail = stepworkColors [ stepworkResource.ColorId ];
          }
        }
      }
    }
    return groupTaskResources;
  }

  private List<ChartStepwork> GrouptaskResourcesToChartStepworks( IList<GroupTaskDetailResource> grouptasks, double amplifiedFactor, bool includeYear )
  {
    var i = includeYear ? 11 : 10;
    var data = new List<ChartStepwork>();
    foreach ( var grouptask in grouptasks ) {
      i++;
      foreach ( var task in grouptask.Tasks ) {
        if ( task.Stepworks.Count > 1 && task.NumberOfTeam > 0 ) {
          var groupByRow = task.Stepworks.GroupBy( x => x.IsSubStepwork );
          foreach ( var group in groupByRow ) {
            CalculateStartDay( group, amplifiedFactor, task.NumberOfTeam, task.Duration );
          }
        }
        task.AmplifiedDuration = task.Duration * ( task.NumberOfTeam == 0 ? 1 : ( amplifiedFactor / task.NumberOfTeam ) );

        foreach ( var sw in task.Stepworks ) {
          var chartSw = new ChartStepwork()
          {
            StepWorkId = sw.StepworkId,
            Color = WorksheetFormater.GetColor( sw.ColorDetail.Code ),
            Start = sw.Start,
            Duration = sw.Portion * task.Duration * ( task.NumberOfTeam == 0 ? 1 : ( amplifiedFactor / task.NumberOfTeam ) ),
            RowIndex = sw.IsSubStepwork ? i + 1 : i
          };
          foreach ( var predecessor in sw.Predecessors ) {
            chartSw.Predecessors.Add(
              new ChartPredecessor()
              {
                RelatedProcessorStepWork = predecessor.RelatedStepworkId,
                Lag = predecessor.Lag,
                Type = sw.Predecessors.Count != 0 ? ( ExportExcel.PredecessorType ) sw.Predecessors.First().Type : ExportExcel.PredecessorType.FinishToStart
              } );
          }
          data.Add( chartSw );
        }
        if ( task.Stepworks.Any( x => x.IsSubStepwork ) ) {
          i++;
        }
        i++;
      }
    }
    return data;
  }

  private void CalculateStartDay( IEnumerable<StepworkDetailResource> stepworks, double amplifiedFactor, int numberOfTeams, double duration )
  {
    var factor = amplifiedFactor - 1;
    var firstStep = stepworks.ElementAt( 0 );
    var gap = duration * firstStep.Portion * factor;
    if ( numberOfTeams > 0 )
      gap /= numberOfTeams;
    for ( int i = 1; i < stepworks.Count(); i++ ) {
      var stepwork = stepworks.ElementAt( i );
      stepwork.Start += gap;
      if ( numberOfTeams > 1 )
        gap += stepwork.Portion * duration * factor;
      else
        gap += stepwork.Portion * duration * factor / numberOfTeams;
    }
  }

  private async Task<IList<ProjectBackgroundResource>> GetBackgrounds( long versionId )
  {
    var colors = await _colorRepository.GetBackgroundColorsByVersionId( versionId );
    var backgrounds = await _backgroundRepository.GetBackgroundsByVersionId( versionId );
    var orderedBackgrounds = backgrounds.OrderBy( x => x.Year ).ThenBy( x => x.Month ).ThenBy( x => x.Date );
    var resources = _mapper.Map<IEnumerable<ProjectBackgroundResource>>( orderedBackgrounds );
    foreach ( var background in resources ) {
      if ( background.ColorId == null ) {
        continue;
      }
      var color = colors.FirstOrDefault( c => c.ColorId == background.ColorId );
      if ( color == null ) {
        continue;
      }
      background.ColorCode = color.Code;
      background.Name = color.Name;
    }
    return resources.ToList();
  }

  #region Get view task
  private void CalculateDuration( IEnumerable<ViewTaskDetail> tasks, ProjectSetting setting )
  {
    var tasksByGroup = tasks.GroupBy( t => t.Group );
    foreach ( var group in tasksByGroup ) {
      if ( group.Count() < 1 ) {
        continue;
      }
      if ( group.Key == 0 ) {
        // not in group
        foreach ( var task in group ) {
          var minStart = task.Stepworks.Min( s => s.Start );
          var maxEnd = task.Stepworks.Max( s => s.End );
          task.MinStart = minStart;
          task.MaxEnd = maxEnd;
          var duration = task.MaxEnd - task.MinStart;
          if ( task.Stepworks.Count > 1 && task.NumberOfTeam != 0 ) {
            // more than one stepwork and number of teams > 0
            for ( int i = 0; i < task.Stepworks.Count - 1; i++ ) {
              duration += task.Stepworks.ElementAt( i ).Duration * ( setting!.AmplifiedFactor - 1 ) / task.NumberOfTeam;
            }
            task.Duration = duration;
          }
          else {
            // other cases
            task.Duration = duration;
          }
        }
      }
      else {
        // in group
        foreach ( var task in group ) {
          if ( !( task.Stepworks.Count > 1 && task.NumberOfTeam != 0 ) ) {
            continue;
          }
          //Recalculate end of last stepwork if task has more than one stepwork
          var gap = 0d;
          for ( int i = 0; i < task.Stepworks.Count - 1; i++ ) {
            gap += task.Stepworks.ElementAt( i ).Duration * ( setting!.AmplifiedFactor - 1 ) / task.NumberOfTeam;
          }
          task.Stepworks.Last().End += gap;
        }
        var stepworks = group.SelectMany( task => task.Stepworks );
        var minStart = stepworks.Min( s => s.Start );
        var maxEnd = stepworks.Max( s => s.End );
        var lastTask = group.First( t => t.Stepworks.Any( s => s.End == maxEnd ) );
        var duration = maxEnd - minStart;

        foreach ( var task in group ) {
          task.MinStart = minStart;
          task.MaxEnd = maxEnd;
          task.Duration = duration;
        }
      }
    }
  }

  public async Task<IEnumerable<ViewTaskDetail>> GetViewTasks( long versionId, long viewId )
  {
    var setting = await _projectSettingRepository.GetByVersionId( versionId );
    var viewtasks = await _viewRepository.GetViewTasks( versionId, viewId );
    foreach ( var task in viewtasks ) {
      var stepworks = await _stepworkRepository.GetStepworksByTaskId( task.TaskId );
      task.Stepworks = stepworks.ToList();
    }
    CalculateDuration( viewtasks, setting! );
    return viewtasks.Where( x => !x.IsHidden );
  }
  #endregion
}
