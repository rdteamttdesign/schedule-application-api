using AutoMapper;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources.FormBody;
using SchedulingTool.Api.Resources;
using System.Text.RegularExpressions;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;
using Task = System.Threading.Tasks.Task;
using Version = SchedulingTool.Api.Domain.Models.Version;
using SchedulingTool.Api.Resources.Extended;
using SchedulingTool.Api.Domain.Models.Enum;
using System.Data;

namespace SchedulingTool.Api.Services;

public class VersionService : IVersionService
{
  private readonly IProjectRepository _projectRepository;
  private readonly IProjectVersionRepository _projectVersionRepository;
  private readonly IVersionRepository _versionRepository;
  private readonly IUnitOfWork _unitOfWork;

  private readonly IProjectSettingRepository _projectSettingRepository;
  private readonly IGroupTaskRepository _groupTaskRepository;
  private readonly ITaskRepository _taskRepository;
  private readonly IStepworkRepository _stepworkRepository;
  private readonly IPredecessorRepository _predecessorRepository;
  private readonly IColorDefRepository _colorRepository;
  private readonly IBackgroundRepository _backgroundRepository;
  private readonly IViewRepository _viewRepository;
  private readonly IViewTaskRepository _viewTaskRepository;
  private readonly IMapper _mapper;

  public VersionService(
    IProjectRepository projectRepository,
    IProjectVersionRepository projectVersionRepository,
    IVersionRepository versionRepository,
    IUnitOfWork unitOfWork,
    IProjectSettingRepository projectSettingRepository,
    IGroupTaskRepository groupTaskRepository,
    ITaskRepository taskRepository,
    IStepworkRepository stepworkRepository,
    IPredecessorRepository predecessorRepository,
    IColorDefRepository colorDefRepository,
    IBackgroundRepository backgroundRepository,
    IViewRepository viewRepository,
    IViewTaskRepository viewTaskRepository,
    IMapper mapper )
  {
    _projectRepository = projectRepository;
    _projectVersionRepository = projectVersionRepository;
    _versionRepository = versionRepository;
    _unitOfWork = unitOfWork;
    _projectSettingRepository = projectSettingRepository;
    _groupTaskRepository = groupTaskRepository;
    _taskRepository = taskRepository;
    _stepworkRepository = stepworkRepository;
    _predecessorRepository = predecessorRepository;
    _colorRepository = colorDefRepository;
    _backgroundRepository = backgroundRepository;
    _viewRepository = viewRepository;
    _viewTaskRepository = viewTaskRepository;
    _mapper = mapper;
  }

  public async Task<IEnumerable<string>> GetActiveVersionNameList( long userId, long projectId )
  {
    var project = await _projectRepository.GetById( projectId );
    if ( project == null ) {
      return Array.Empty<string>();
    }
    var versions = project.IsShared
      ? await _projectRepository.GetSharedProjectVersionDetails()
      : await _projectRepository.GetProjectVersionDetails( userId );
    return versions.Where( x => x.ProjectId == projectId && x.IsActivated ).Select( x => x.VersionName ).Distinct();
  }

  public async Task BatchDeleteVersionDetails( long versionId )
  {
    await _versionRepository.BatchDeleteVersionDetails( versionId );
  }

  public async Task<Version?> GetVersionById( long versionId )
  {
    var version = await _versionRepository.GetById( versionId );
    return ( version?.IsActivated ?? false ) ? version : null;
  }

  public async Task<ServiceResponse<Version>> CreateVersion( long projectId, Version version )
  {
    try {
      var newVersion = await _versionRepository.Create( version );
      await _unitOfWork.CompleteAsync();
      await _projectVersionRepository.Create( new ProjectVersion() { ProjectId = projectId, VersionId = version.VersionId } );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Version>( newVersion );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Version>( $"{ProjectNotification.ErrorSaving} {ex.Message}. {ex.InnerException?.Message}" );
    }
  }

  public async Task BatchDeactiveVersions( ICollection<long> versionIds, string userName )
  {
    var versionsToDeactive = await _versionRepository.GetVersionsById( versionIds );
    if ( !versionsToDeactive.Any() )
      return;

    foreach ( var version in versionsToDeactive ) {
      version.IsActivated = false;
      version.ModifiedDate = DateTime.UtcNow;
      version.ModifiedBy = userName;
      await _versionRepository.Update( version );
      await _unitOfWork.CompleteAsync();
    }
  }

  public async Task<ServiceResponse<Version>> UpdateVersion( Version version )
  {
    try {
      await _versionRepository.Update( version );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Version>( version );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Version>( $"{ProjectNotification.ErrorSaving} {ex.Message}. {ex.InnerException?.Message}" );
    }
  }

  public async Task BatchDeleteVersions( ICollection<long> versionIds )
  {
    var projectIdList = ( await _projectVersionRepository.GetAll() ).Where( x => versionIds.Contains( x.VersionId ) ).Select( x => x.ProjectId ).Distinct();
    foreach ( var versionId in versionIds ) {
      await _versionRepository.BatchDeleteVersion( versionId );
    }
  }

  public async Task BatchActivateVersions( long userId, string userName, ICollection<long> versionIds )
  {
    var projects = await _projectRepository.GetAllProjects( userId );
    var activatedVersionIds = ( await _versionRepository.GetActiveVersions( userId ) ).Select( x => x.VersionId );
    var projectVersions = await _projectVersionRepository.GetAll();
    var activatedProjectVersions = projectVersions.Where( x => activatedVersionIds.Contains( x.VersionId ) );
    var projectInTrashBinIdList = projectVersions.Where( x => versionIds.Contains( x.VersionId ) ).Select( x => x.ProjectId );
    var projectIdsToActivate = projectInTrashBinIdList.Where( x => !activatedProjectVersions.Any( pv => pv.ProjectId == x ) ).Distinct();
    var activatedProjects = projects.Where( x => activatedProjectVersions.Any( pv => pv.ProjectId == x.ProjectId ) );

    foreach ( var projectId in projectIdsToActivate ) {
      var project = await _projectRepository.GetById( projectId );
      if ( activatedProjects.All( p => p.ProjectId != projectId ) ) {
        if ( activatedProjects.Any( p => p.ProjectName == project.ProjectName ) ) {
          project.ProjectName += $" (restored at {( int ) DateTime.UtcNow.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds})";
        }
      }
      project.ModifiedDate = DateTime.UtcNow;
      await _projectRepository.Update( project );
    }

    var versionsToActivate = await _versionRepository.GetVersionsById( versionIds );
    if ( !versionsToActivate.Any() )
      return;

    foreach ( var version in versionsToActivate ) {
      version.VersionName += $" (restored at {( int ) DateTime.UtcNow.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds})";
      version.ModifiedDate = DateTime.UtcNow;
      version.ModifiedBy = userName;
      version.IsActivated = true;
      await _versionRepository.Update( version );
    }
    await _unitOfWork.CompleteAsync();
  }

  public async Task<IEnumerable<object>> GetGroupTasksByVersionId( long versionId, int columnWidth, double amplifiedFactor )
  {
    var setting = await _projectSettingRepository.GetByVersionId( versionId );
    if ( columnWidth == -1 )
      columnWidth = setting!.ColumnWidth;
    if ( amplifiedFactor == -1 )
      amplifiedFactor = setting!.AmplifiedFactor;

    var result = new List<KeyValuePair<int, object>>();
    var groupTasks = await _groupTaskRepository.GetGroupTasksByVersionId( versionId );
    var groupTaskResources = _mapper.Map<List<GroupTaskResource>>( groupTasks );
    groupTaskResources.ForEach( g => result.Add( new KeyValuePair<int, object>( g.DisplayOrder, g ) ) );

    foreach ( var groupTask in groupTasks ) {
      var tasks = await _taskRepository.GetTasksByGroupTaskId( groupTask.GroupTaskId );
      foreach ( var task in tasks ) {

        var stepworks = await _stepworkRepository.GetStepworksByTaskId( task.TaskId );
        var taskResource = _mapper.Map<TaskResource>( task );
        taskResource.Duration = task.Duration;

        if ( stepworks.Count() == 1 ) {
          var predecessors = await _predecessorRepository.GetPredecessorsByStepworkId( stepworks.First().StepworkId );
          var predecessorResources = _mapper.Map<List<PredecessorResource>>( predecessors );
          taskResource.Predecessors = predecessorResources.Count == 0 ? null : predecessorResources;
          taskResource.ColorId = stepworks.First().ColorId;
          taskResource.Start = stepworks.First().Start.DaysToColumnWidth( columnWidth );
          taskResource.End = taskResource.Start
            + task.Duration.DaysToColumnWidth( columnWidth ) * ( task.NumberOfTeam == 0 ? 1 : amplifiedFactor );
        }
        else {
          if ( task.NumberOfTeam != 0 ) {
            // --->comment old code
            //var factor = amplifiedFactor - 1;
            //var firstStep = stepworks.ElementAt( 0 );
            //var gap = firstStep.Duration * factor;
            //if ( task.NumberOfTeam > 0 )
            //  gap /= task.NumberOfTeam;
            //for ( int i = 1; i < stepworks.Count(); i++ ) {
            //  var stepwork = stepworks.ElementAt( i );
            //  stepwork.Start += gap;
            //  if ( task.NumberOfTeam > 1 )
            //    gap += stepwork.Duration * factor;
            //  else
            //    gap += stepwork.Duration * factor / task.NumberOfTeam;
            //}
            // ---> end comment

            var groupByRow = stepworks.GroupBy( x => x.IsSubStepwork );
            foreach ( var group in groupByRow ) {
              CalculateStartDay( group, amplifiedFactor, task.NumberOfTeam );
            }
          }

          var stepworkResources = new List<StepworkResource>();
          foreach ( var stepwork in stepworks ) {
            if ( stepwork.Portion == 1 ) {
              continue;
            }
            var predecessors = await _predecessorRepository.GetPredecessorsByStepworkId( stepwork.StepworkId );
            var predecessorResources = _mapper.Map<List<PredecessorResource>>( predecessors );
            var stepworkResource = _mapper.Map<StepworkResource>( stepwork );
            stepworkResource.Duration = task.Duration;
            stepworkResource.PercentStepWork *= 100;
            stepworkResource.Start = stepwork.Start.DaysToColumnWidth( columnWidth );
            stepworkResource.End = stepworkResource.Start
              + stepworkResource.Duration.DaysToColumnWidth( columnWidth ) * ( task.NumberOfTeam == 0 ? 1 : amplifiedFactor );
            stepworkResource.GroupId = groupTask.LocalId;
            stepworkResource.Predecessors = predecessorResources.Count == 0 ? null : predecessorResources;
            stepworkResource.GroupNumbers = task.NumberOfTeam;
            stepworkResources.Add( stepworkResource );
          }
          taskResource.Stepworks = stepworkResources.Count == 0 ? null : stepworkResources;
        }
        result.Add( new KeyValuePair<int, object>( taskResource.DisplayOrder, taskResource ) );
      }
    }
    return result.OrderBy( o => o.Key ).Select( o => o.Value );
  }

  private bool StepworksOverlap( Stepwork first, Stepwork second, bool invert = false )
  {
    var secondEnd = second.Start + second.Duration;
    if ( first.Start >= second.Start && first.Start < secondEnd ) {
      return true;
    }
    if ( invert && StepworksOverlap( second, first ) ) {
      return true;
    }

    return false;
  }

  private void CalculateStartDay( IEnumerable<Stepwork> stepworks, double amplifiedFactor, int numberOfTeams )
  {
    var factor = amplifiedFactor - 1;
    var firstStep = stepworks.ElementAt( 0 );
    var gap = firstStep.Duration * factor;
    if ( numberOfTeams > 0 )
      gap /= numberOfTeams;
    for ( int i = 1; i < stepworks.Count(); i++ ) {
      // TODO: Fix bug stepwork
      bool sameStart = i < stepworks.Count() - 1 && StepworksOverlap( stepworks.ElementAt( i ), stepworks.ElementAt( i + 1 ), true );
      var stepwork = stepworks.ElementAt( i );
      stepwork.Start += gap;
      if ( !sameStart )
        if ( numberOfTeams > 1 )
          gap += stepwork.Duration * factor;
        else
          gap += stepwork.Duration * factor / numberOfTeams;
    }
  }

  public async Task SaveProjectTasks( long versionId, ICollection<CommonGroupTaskFormData> formData )
  {
    var setting = await _projectSettingRepository.GetByVersionId( versionId );
    var stepworkColor = await _colorRepository.GetStepworkColorsByVersionId( versionId );
    var installColor = stepworkColor.First( c => c.IsInstall == 0 );
    var converter = new FormDataToModelConverter( versionId, setting!, installColor!, formData );

    var grouptasks = new Dictionary<string, GroupTask>();
    foreach ( var grouptask in converter.GroupTasks ) {
      var result = await CreateGroupTask( grouptask );
      if ( result.Success ) {
        grouptasks.Add( result.Content.LocalId, result.Content );
      }
    }

    var tasks = new Dictionary<string, ModelTask>();
    foreach ( var task in converter.Tasks ) {
      task.GroupTaskId = grouptasks [ task.GroupTaskLocalId ].GroupTaskId;
      var result = await CreateTask( task );
      if ( result.Success ) {
        tasks.Add( result.Content.LocalId, result.Content );
      }
    }

    var stepworks = new Dictionary<string, Stepwork>();
    foreach ( var stepwork in converter.Stepworks ) {
      stepwork.TaskId = tasks [ stepwork.TaskLocalId ].TaskId;
      //stepwork.End = stepwork.Start + stepwork.Duration;
      var result = await CreateStepwork( stepwork );
      if ( result.Success ) {
        stepworks.Add( result.Content.LocalId, result.Content );
      }
    }

    foreach ( var predecessor in converter.Predecessors ) {
      if ( !stepworks.ContainsKey( predecessor.StepworkId ) || !stepworks.ContainsKey( predecessor.RelatedStepworkId ) ) {
        continue;
      }
      await CreatePredecessor( new Predecessor()
      {
        StepworkId = stepworks [ predecessor.StepworkId ].StepworkId,
        RelatedStepworkId = stepworks [ predecessor.RelatedStepworkId ].StepworkId,
        RelatedStepworkLocalId = predecessor.RelatedStepworkId,
        Type = predecessor.Type,
        Lag = predecessor.Lag
      } );
    }
  }

  public async Task<ServiceResponse<VersionResource>> DuplicateVersion( long userId, string userName, long projectId, Version oldVersion, string? newVersionName )
  {
    if ( newVersionName == null ) {
      var latestNameList = await GetActiveVersionNameList( userId, projectId );
      newVersionName = NumberingNewName( latestNameList, $"Copy of {oldVersion.VersionName}" );
    }

    var newVersion = new Version()
    {
      VersionName = newVersionName,
      UserId = userId,
      CreatedDate = DateTime.UtcNow,
      ModifiedDate = DateTime.UtcNow,
      ModifiedBy = userName,
      IsActivated = oldVersion.IsActivated,
      NumberOfMonths = oldVersion.NumberOfMonths
    };

    var result = await CreateVersion( projectId, newVersion );
    if ( !result.Success ) {
      return new ServiceResponse<VersionResource>( ProjectNotification.ErrorDuplicating );
    }
    var resource = _mapper.Map<VersionResource>( result.Content );

    var setting = await _projectSettingRepository.GetByVersionId( oldVersion.VersionId );
    var newSetting = await _projectSettingRepository.GetByVersionId( result.Content.VersionId );
    newSetting!.ColumnWidth = setting!.ColumnWidth;
    newSetting.SeparateGroupTask = setting.SeparateGroupTask;
    newSetting.AssemblyDurationRatio = setting.AssemblyDurationRatio;
    newSetting.RemovalDurationRatio = setting.RemovalDurationRatio;
    newSetting.ColumnWidth = setting.ColumnWidth;
    newSetting.AmplifiedFactor = setting.AmplifiedFactor;
    newSetting.IncludeYear = setting.IncludeYear;
    newSetting.StartMonth = setting.StartMonth;
    newSetting.StartYear = setting.StartYear;
    if ( newSetting != null ) {
      await _projectSettingRepository.Update( newSetting );
      await _unitOfWork.CompleteAsync();
    }

    #region Duplicate color
    var bgColorList = new Dictionary<long, ColorDef>();
    var swColorList = new Dictionary<long, ColorDef>();

    var swColors = await _colorRepository.GetStepworkColorsByVersionId( oldVersion.VersionId );
    var newSwColors = await _colorRepository.GetStepworkColorsByVersionId( result.Content.VersionId );
    foreach ( var color in swColors ) {
      if ( color.IsDefault ) {
        if ( color.IsInstall == 0 || color.IsInstall == 1 ) {
          var newDefColor = newSwColors.FirstOrDefault( x => x.IsInstall == color.IsInstall );
          newDefColor!.Code = color.Code;
          await _colorRepository.Update( newDefColor );
          await _unitOfWork.CompleteAsync();
          swColorList.Add( color.ColorId, newDefColor );
        }
        continue;
      }

      var newColor = new ColorDef()
      {
        Name = color.Name,
        Code = color.Code,
        Type = color.Type,
        VersionId = result.Content.VersionId,
        IsDefault = color.IsDefault,
        IsInstall = color.IsInstall
      };
      var newColorResult = await CreateColorDef( newColor );
      swColorList.Add( color.ColorId, newColorResult.Content );
    }
    var bgColors = await _colorRepository.GetBackgroundColorsByVersionId( oldVersion.VersionId );
    foreach ( var color in bgColors ) {
      var newColor = new ColorDef()
      {
        Name = color.Name,
        Code = color.Code,
        Type = color.Type,
        VersionId = result.Content.VersionId,
        IsDefault = color.IsDefault,
        IsInstall = color.IsInstall
      };
      var newColorResult = await CreateColorDef( newColor );
      bgColorList.Add( color.ColorId, newColorResult.Content );
    }

    var backgrounds = await _backgroundRepository.GetBackgroundsByVersionId( oldVersion.VersionId );
    foreach ( var bg in backgrounds ) {
      var updatingBackground = await _backgroundRepository.GetProjectBackground( result.Content.VersionId, bg.Year, bg.Month, bg.Date );
      if ( updatingBackground == null ) {
        continue;
      }
      if ( bg.ColorId == null ) {
        continue;
      }
      updatingBackground.ColorId = bgColorList [ bg.ColorId.Value ].ColorId;
      await _backgroundRepository.Update( updatingBackground );
      await _unitOfWork.CompleteAsync();
    }
    #endregion

    #region Duplicate details
    var predecessorList = new List<Predecessor>();
    var stepworkIdList = new Dictionary<long, long>();
    var groupTasks = await _groupTaskRepository.GetGroupTasksByVersionId( oldVersion.VersionId );
    foreach ( var groupTask in groupTasks ) {
      var newGroupTask = new GroupTask
      {
        GroupTaskId = 0,
        VersionId = result.Content.VersionId,
        GroupTaskName = groupTask.GroupTaskName,
        Index = groupTask.Index,
        HideChidren = groupTask.HideChidren,
        LocalId = groupTask.LocalId
      };
      var newGroupTaskResult = await CreateGroupTask( newGroupTask );
      if ( !newGroupTaskResult.Success ) {
        continue;
      }
      var tasks = await _taskRepository.GetTasksByGroupTaskId( groupTask.GroupTaskId );

      foreach ( var task in tasks ) {
        var newTask = new ModelTask
        {
          TaskId = 0,
          GroupTaskId = newGroupTaskResult.Content.GroupTaskId,
          TaskName = task.TaskName,
          Index = task.Index,
          NumberOfTeam = task.NumberOfTeam,
          Duration = task.Duration,
          AmplifiedDuration = task.AmplifiedDuration,
          Description = task.Description,
          Note = task.Note,
          LocalId = task.LocalId,
          GroupTaskLocalId = task.GroupTaskLocalId
        };

        var newTaskResult = await CreateTask( newTask );
        if ( !newTaskResult.Success ) {
          continue;
        }
        var stepworks = await _stepworkRepository.GetStepworksByTaskId( task.TaskId );

        foreach ( var stepwork in stepworks ) {
          var newStepwork = new Stepwork
          {
            StepworkId = 0,
            TaskId = newTaskResult.Content.TaskId,
            Index = stepwork.Index,
            Portion = stepwork.Portion,
            ColorId = swColorList [ stepwork.ColorId ].ColorId,
            LocalId = stepwork.LocalId,
            TaskLocalId = stepwork.TaskLocalId,
            Name = stepwork.Name,
            Start = stepwork.Start,
            End = stepwork.End,
            Duration = stepwork.Duration,
            Type = stepwork.Type,
            IsSubStepwork = stepwork.IsSubStepwork
          };
          var newStepworkResult = await CreateStepwork( newStepwork );
          if ( !newStepworkResult.Success ) {
            continue;
          }
          stepworkIdList.Add( stepwork.StepworkId, newStepworkResult.Content.StepworkId );
          var predecessors = await _predecessorRepository.GetPredecessorsByStepworkId( stepwork.StepworkId );
          predecessorList.AddRange( predecessors );
        }
      }
    }

    foreach ( var predecessor in predecessorList ) {
      if ( predecessor == null ) {
        continue;
      }
      if ( !( stepworkIdList.ContainsKey( predecessor.StepworkId ) && stepworkIdList.ContainsKey( predecessor.RelatedStepworkId ) ) ) {
        continue;
      }
      predecessor.StepworkId = stepworkIdList [ predecessor.StepworkId ];
      predecessor.RelatedStepworkId = stepworkIdList [ predecessor.RelatedStepworkId ];
      await CreatePredecessor( predecessor );
    }
    #endregion

    await DuplicateView( oldVersion.VersionId, result.Content.VersionId );

    return new ServiceResponse<VersionResource>( resource );
  }

  private async Task DuplicateView( long fromVersionId, long toVersionId )
  {
    var existingViews = await _viewRepository.GetViewsByVersionId( fromVersionId );
    foreach ( var existingView in existingViews ) {
      var newView = new View()
      {
        ViewName = existingView.ViewName,
        VersionId = toVersionId
      };
      var newViewResult = await _viewRepository.Create( newView );
      await _unitOfWork.CompleteAsync();
      if ( newViewResult == null ) {
        continue;
      }
      var viewtasks = await _viewTaskRepository.GetViewTasksByViewId( existingView.ViewId );
      foreach ( var task in viewtasks ) {
        var newViewTask = new ViewTask()
        {
          LocalTaskId = task.LocalTaskId,
          Group = task.Group,
          DisplayOrder = task.DisplayOrder,
          ViewId = newView.ViewId,
          IsHidden = task.IsHidden,
          TaskName = task.TaskName,
          TaskDescription = task.TaskDescription,
          TaskNote = task.TaskNote,
          IsDayFormat = task.IsDayFormat
        };
        await _viewTaskRepository.Create( newViewTask );
      }
      await _unitOfWork.CompleteAsync();
    }
  }

  private string NumberingNewName( IEnumerable<string> existingNamList, string newName )
  {
    var latestName = existingNamList.Where( name => name.Length > newName.Length )
      .Where( name => name.Substring( 0, newName.Length + 1 ) == $"{newName} " && Regex.IsMatch( name.Split( " " ).LastOrDefault() ?? string.Empty, @"\([1-9]+\)" ) )
      .OrderByDescending( x => x ).FirstOrDefault();
    if ( latestName != null ) {
      var duplicatedNumber = latestName.Replace( $"{newName} ", "" ).Replace( "(", "" ).Replace( ")", "" );
      int.TryParse( duplicatedNumber, out var digit );
      newName += $" ({digit + 1})";
    }
    else {
      newName += " (1)";
    }
    return newName;
  }

  private async Task<ServiceResponse<GroupTask>> CreateGroupTask( GroupTask groupTask )
  {
    try {
      var newGroupTask = await _groupTaskRepository.Create( groupTask );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<GroupTask>( newGroupTask );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<GroupTask>( $"{GroupTaskNotification.ErrorSaving} {ex.Message}" );
    }
  }

  private async Task<ServiceResponse<ModelTask>> CreateTask( ModelTask task )
  {
    try {
      var newTask = await _taskRepository.Create( task );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ModelTask>( newTask );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ModelTask>( $"{TaskNotification.ErrorSaving} {ex.Message}" );
    }
  }

  private async Task<ServiceResponse<Stepwork>> CreateStepwork( Stepwork stepwork )
  {
    try {
      var newStepwork = await _stepworkRepository.Create( stepwork );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Stepwork>( newStepwork );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Stepwork>( $"{StepworkNotification.ErrorSaving} {ex.Message}" );
    }
  }

  private async Task<ServiceResponse<Predecessor>> CreatePredecessor( Predecessor predecessors )
  {
    try {
      var newPredecessor = await _predecessorRepository.Create( predecessors );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Predecessor>( newPredecessor );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Predecessor>( $"{PredecessorNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<ColorDef>> CreateColorDef( ColorDef colorDef )
  {
    try {
      var newColor = await _colorRepository.Create( colorDef );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ColorDef>( newColor );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ColorDef>( $"{ColorDefNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<dynamic> GetDataFromFile( long versionId, Stream fileStream, int maxDisplayOrder, string [] sheetNameList )
  {
    var colors = await _colorRepository.GetStepworkColorsByVersionId( versionId );
    var installColor = colors.FirstOrDefault( c => c.IsInstall == 0 );
    var removalColor = colors.FirstOrDefault( c => c.IsInstall == 1 );
    var setting = await _projectSettingRepository.GetByVersionId( versionId );
    var result = ExcelFileReader.ReadFromFile(
      fileStream,
      sheetNameList,
      setting!,
      installColor!.ColorId,
      removalColor!.ColorId,
      maxDisplayOrder,
      out var messages );

    return new { Messages = messages, Result = result };
  }

  public async Task<dynamic> GetUpdatedDataFromFile( long versionId, Stream fileStream, string [] sheetNameList )
  {
    var colors = await _colorRepository.GetStepworkColorsByVersionId( versionId );
    var installColor = colors.FirstOrDefault( c => c.IsInstall == 0 );
    var removalColor = colors.FirstOrDefault( c => c.IsInstall == 1 );
    var setting = await _projectSettingRepository.GetByVersionId( versionId );
    var grouptasksFromFile = ExcelFileReader.ReadFromFile( fileStream, sheetNameList, installColor!.ColorId, removalColor!.ColorId, out _ );

    if ( grouptasksFromFile.Count == 0 ) {
      return new List<UpdateGrouptaskResource>();
    }

    var existingGrouptasks = await GetGroupTasksByVersionId( versionId );

    return CompareGrouptasksByOrder( existingGrouptasks, grouptasksFromFile );
  }

  public async Task<List<GroupTaskDetailResource>> GetGroupTasksByVersionId( long versionId )
  {
    var groupTasks = await _groupTaskRepository.GetGroupTasksByVersionId( versionId );
    var groupTaskResources = _mapper.Map<List<GroupTaskDetailResource>>( groupTasks );

    foreach ( var groupTask in groupTaskResources ) {
      var tasks = await _taskRepository.GetTasksByGroupTaskId( groupTask.GroupTaskId!.Value );
      var taskResources = _mapper.Map<List<TaskDetailResource>>( tasks );
      groupTask.Tasks = taskResources;
      foreach ( var task in taskResources ) {
        var stepworks = await _stepworkRepository.GetStepworksByTaskId( task.TaskId!.Value );
        var stepworkResources = _mapper.Map<List<StepworkDetailResource>>( stepworks );
        task.Stepworks = stepworkResources;
      }
    }
    return groupTaskResources;
  }

  public IList<UpdateGrouptaskResource> CompareGrouptasksByOrder( IList<GroupTaskDetailResource> first, IList<GroupTaskDetailResource> second )
  {
    var result = new List<UpdateGrouptaskResource>();

    foreach ( var updatingGroup in second ) {
      var existingGroup = first.FirstOrDefault( x => x.GroupTaskName == updatingGroup.GroupTaskName );
      if ( existingGroup == null ) {
        #region New grouptask
        var resource = new UpdateGrouptaskResource()
        {
          Change = DataChange.New,
          Id = Guid.NewGuid().ToString(),
          Name = updatingGroup.GroupTaskName
        };

        result.Add( resource );

        if ( updatingGroup.Tasks.Count == 0 ) {
          continue;
        }
        resource.Tasks = new List<UpdateTaskResource>();
        foreach ( var task in updatingGroup.Tasks ) {
          resource.Tasks.Add( CreateUpdateResource( task, DataChange.New ) );
        }
        continue;
        #endregion
      }

      var changes = CompareTasksByOrder( existingGroup.Tasks, updatingGroup.Tasks );

      if ( changes.All( x => x.Change == DataChange.None ) ) {
        #region No change
        result.Add( new UpdateGrouptaskResource()
        {
          Change = DataChange.None,
          Id = existingGroup.LocalId,
          Name = existingGroup.GroupTaskName,
          Tasks = null
        } );
        #endregion
      }
      else {
        #region Update grouptask
        result.Add( new UpdateGrouptaskResource()
        {
          Change = DataChange.Update,
          Id = existingGroup.LocalId,
          Name = existingGroup.GroupTaskName,
          Tasks = changes
        } );
        #endregion
      }
      first.Remove( existingGroup );
    }

    foreach ( var remainExistingGroup in first ) {
      #region Delete grouptask
      result.Add( new UpdateGrouptaskResource()
      {
        Change = DataChange.Delete,
        Id = remainExistingGroup.LocalId,
        Name = remainExistingGroup.GroupTaskName,
        Tasks = null
      } );
      #endregion
    }
    return result;
  }

  public IList<UpdateTaskResource> CompareTasksByOrder( IList<TaskDetailResource> first, IList<TaskDetailResource> second )
  {
    var result = new List<UpdateTaskResource>();

    foreach ( var updatingTask in second ) {
      var existingTask = first.FirstOrDefault( x => x.TaskName == updatingTask.TaskName && x.Description == updatingTask.Description );
      if ( existingTask == null ) {
        updatingTask.LocalId = Guid.NewGuid().ToString();
        result.Add( CreateUpdateResource( updatingTask, DataChange.New ) );
        continue;
      }
      if ( existingTask.Equals( updatingTask ) ) {
        result.Add( CreateUpdateResource( existingTask, DataChange.None ) );
      }
      else {
        if ( updatingTask.Stepworks.Count != existingTask.Stepworks.Count ) {
          // number of stepworks has changed, delete existing, add updating task
          result.Add( CreateUpdateResource( existingTask, DataChange.Delete ) );
          result.Add( CreateUpdateResource( updatingTask, DataChange.New ) );
        }
        else {
          updatingTask.LocalId = existingTask.LocalId;
          var preservedStepworkCount = Math.Min( updatingTask.Stepworks.Count, existingTask.Stepworks.Count );

          for ( int i = 0; i < preservedStepworkCount; i++ )
            updatingTask.Stepworks [ i ].LocalId = existingTask.Stepworks [ i ].LocalId;

          result.Add( CreateUpdateResource( updatingTask, DataChange.Update ) );
        }
      }
      first.Remove( existingTask );
    }

    foreach ( var remainExistingTask in first ) {
      result.Add( CreateUpdateResource( remainExistingTask, DataChange.Delete ) );
    }

    return result;
  }

  private UpdateTaskResource CreateUpdateResource( TaskDetailResource resource, string change )
  {
    var result = _mapper.Map<UpdateTaskResource>( resource );
    if ( resource.Stepworks.Count > 1 && ( change != DataChange.Delete && change != DataChange.None ) ) {
      result.Stepworks = _mapper.Map<List<UpdateStepworkResource>>( resource.Stepworks );
      foreach ( var stepwork in result.Stepworks ) {
        stepwork.PercentStepWork *= 100;
      }
    }
    else {
      result.Stepworks = null;
    }
    result.Change = change;
    return result;
  }

  public async Task SendVersionsToAnotherProject( long userId, string userName, SendVersionsToAnotherProjectFormData formData )
  {
    var myProjectList = await _projectRepository.GetProjectVersionDetails( userId );
    var sharedProjectList = await _projectRepository.GetSharedProjectVersionDetails();

    if ( formData.ToProjectId == null ) {
      // Create new project
      var toProjectList = formData.SendToSharedProjectList ? sharedProjectList : myProjectList;

      var projectNameList = toProjectList.Where( p => p.IsActivated ).Select( p => p.ProjectName ).Distinct();
      var newProjectName = NumberingNewName( projectNameList, "新しいプロジェクト" );
      var project = new Project()
      {
        ProjectName = newProjectName,
        UserId = userId,
        CreatedDate = DateTime.UtcNow,
        ModifiedDate = DateTime.UtcNow,
        ModifiedBy = userName,
        IsShared = formData.SendToSharedProjectList
      };
      var newProject = await _projectRepository.Create( project );
      await _unitOfWork.CompleteAsync();
      formData.ToProjectId = newProject.ProjectId;
    }

    if ( formData.CreateCopy ) {
      foreach ( var versionId in formData.VersionIds ) {
        var oldVersion = await _versionRepository.GetById( versionId );
        var toProjectNameList = await GetActiveVersionNameList( userId, formData.ToProjectId!.Value );
        var newVersionName = toProjectNameList.Any( x => x == oldVersion!.VersionName ) ? null : oldVersion!.VersionName;
        await DuplicateVersion( userId, userName, formData.ToProjectId!.Value, oldVersion!, newVersionName );
      }
    }
    else {
      var fromProjectList = formData.SendToSharedProjectList ? myProjectList : sharedProjectList;
      var fromProjectIdList = fromProjectList.Where( p => formData.VersionIds.Contains( p.VersionId ) ).Select( p => p.ProjectId ).Distinct();

      await MoveVersionsToAnotherProject( userName, formData.VersionIds, formData.ToProjectId!.Value );

      #region Remove project if it is empty
      var projectVersions = await _projectVersionRepository.GetAll();
      var emptyProjectIdList = fromProjectIdList.Where( id => !projectVersions.Any( p => p.ProjectId == id ) );
      
      foreach ( var projectId in emptyProjectIdList ) {
        var project = await _projectRepository.GetById( projectId );
        if ( project == null ) {
          continue;
        }
        _projectRepository.Delete( project );
        await _unitOfWork.CompleteAsync();
      }
      #endregion
    }
  }

  public async Task MoveVersionsToAnotherProject( string userName, ICollection<long> versionIds, long toProjectId )
  {
    var projectVersions = ( await _projectVersionRepository.GetAll() ).Where( x => versionIds.Contains( x.VersionId ) );
    foreach ( var projectVersion in projectVersions ) {
      _projectVersionRepository.Delete( projectVersion );
      await _projectVersionRepository.Create( new ProjectVersion() { ProjectId = toProjectId, VersionId = projectVersion.VersionId } );

      var version = await _versionRepository.GetById( projectVersion.VersionId );
      version!.ModifiedDate = DateTime.UtcNow;
      version.ModifiedBy = userName;
      await _versionRepository.Update( version );
    }
    await _unitOfWork.CompleteAsync();
  }
}
