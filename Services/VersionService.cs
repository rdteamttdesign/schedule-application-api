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
  private readonly IUnitOfWork _unitOfWork;
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
    IUnitOfWork unitOfWork,
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
    _unitOfWork = unitOfWork;
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
      var viewTasks = await _viewRepository.GetViewTasks( versionId, view.ViewId );
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
          var gap = task.Stepworks.First().Portion * task.Duration * ( task.NumberOfTeam == 0 ? 1 : ( ( amplifiedFactor - 1 ) / task.NumberOfTeam ) );
          for ( int j = 1; j < task.Stepworks.Count; j++ ) {
            task.Stepworks.ElementAt( j ).Start += gap;
            gap += task.Stepworks.ElementAt( j ).Portion * task.Duration * ( task.NumberOfTeam == 0 ? 1 : ( ( amplifiedFactor - 1 ) / task.NumberOfTeam ) );
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
            RowIndex = i
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
        i++;
      }
    }
    return data;
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
}
