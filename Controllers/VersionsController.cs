using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using SchedulingTool.Api.Resources.FormBody.projectdetail;
using SchedulingTool.Api.Resources.projectdetail;
using System.Text.RegularExpressions;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;
using Task = System.Threading.Tasks.Task;
using Version = SchedulingTool.Api.Domain.Models.Version;

namespace SchedulingTool.Api.Controllers;

[Route( "api" )]
[ApiController]
public class VersionsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IVersionService _versionService;
  private readonly IProjectSettingService _projectSettingService;
  private readonly IGroupTaskService _groupTaskService;
  private readonly ITaskService _taskService;
  private readonly IStepworkService _stepworkService;
  private readonly IPredecessorService _predecessorService;
  private readonly IColorDefService _colorService;
  private readonly IBackgroundService _backgroundService;
  private readonly IViewService _viewService;
  private readonly IViewTaskService _viewTaskService;

  public VersionsController(
    IMapper mapper,
    IVersionService versionService,
    IProjectSettingService projectSettingService,
    IGroupTaskService groupTaskService,
    ITaskService taskService,
    IStepworkService stepworkService,
    IPredecessorService predecessorService,
    IColorDefService colorService,
    IBackgroundService backgroundService,
    IViewService viewService,
    IViewTaskService viewTaskService )
  {
    _mapper = mapper;
    _versionService = versionService;
    _groupTaskService = groupTaskService;
    _taskService = taskService;
    _stepworkService = stepworkService;
    _predecessorService = predecessorService;
    _colorService = colorService;
    _projectSettingService = projectSettingService;
    _backgroundService = backgroundService;
    _viewService = viewService;
    _viewTaskService = viewTaskService;
  }

  [HttpPut( "versions/deactive-versions" )]
  [Authorize]
  public async Task<IActionResult> DeactiveVersions( [FromBody] ICollection<long> versionIds )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    try {
      await _versionService.BatchDeactiveVersions( userId, versionIds );
      return NoContent();
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ProjectNotification.ErrorSaving} {ex.Message}. {ex.InnerException?.Message}" );
    }
  }

  [HttpPut( "versions/{versionId}" )]
  [Authorize]
  public async Task<IActionResult> UpdateVersion( long versionId, [FromBody] UpdateVersionFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var existingVersion = await _versionService.GetVersionById( versionId );
    if ( existingVersion is null )
      return BadRequest( ProjectNotification.NonExisted );

    existingVersion.VersionName = formData.VersionName;
    existingVersion.ModifiedDate = DateTime.UtcNow;

    var result = await _versionService.UpdateVersion( existingVersion );
    if ( !result.Success )
      return BadRequest( result.Message );

    var resource = _mapper.Map<VersionResource>( result.Content );
    resource.CreatedDate = resource.CreatedDate.ToLocalTime();
    resource.ModifiedDate = resource.ModifiedDate.ToLocalTime();

    return Ok( resource );
  }

  [HttpGet( "versions/{versionId}/details" )]
  [Authorize]
  public async Task<IActionResult> GetProjectDetails( long versionId, int columnWidth, float amplifiedFactor )
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var version = await _versionService.GetVersionById( versionId );
    if ( version == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    try {
      var groupTaskResources = await GetGroupTasksByVersionId( versionId, columnWidth, amplifiedFactor );
      return Ok( groupTaskResources );
    }
    catch ( Exception ex ) {
      return BadRequest( $"Something went wrong: {ex.Message}. {ex.InnerException?.Message}" );
    }
  }

  private async Task<IEnumerable<object>> GetGroupTasksByVersionId( long versionId, int columnWidth, float amplifiedFactor )
  {
    var setting = await _projectSettingService.GetProjectSetting( versionId );
    if ( columnWidth == -1 )
      columnWidth = setting!.ColumnWidth;
    if ( amplifiedFactor == -1 )
      amplifiedFactor = setting!.AmplifiedFactor;

    var result = new List<KeyValuePair<int, object>>();
    var groupTasks = await _groupTaskService.GetGroupTasksByVersionId( versionId );
    var groupTaskResources = _mapper.Map<List<GroupTaskResource>>( groupTasks );
    groupTaskResources.ForEach( g => result.Add( new KeyValuePair<int, object>( g.DisplayOrder, g ) ) );

    foreach ( var groupTask in groupTasks ) {
      var tasks = await _taskService.GetTasksByGroupTaskId( groupTask.GroupTaskId );
      foreach ( var task in tasks ) {

        var stepworks = await _stepworkService.GetStepworksByTaskId( task.TaskId );
        var taskResource = _mapper.Map<TaskResource>( task );
        taskResource.Duration = task.Duration;

        if ( stepworks.Count() == 1 ) {
          var predecessors = await _predecessorService.GetPredecessorsByStepworkId( stepworks.First().StepworkId );
          var predecessorResources = _mapper.Map<List<PredecessorResource>>( predecessors );
          taskResource.Predecessors = predecessorResources.Count == 0 ? null : predecessorResources;
          taskResource.ColorId = stepworks.First().ColorId;
          taskResource.Start = stepworks.First().Start.DaysToColumnWidth( columnWidth );
          taskResource.End = taskResource.Start
            + task.Duration.DaysToColumnWidth( columnWidth ) * ( task.NumberOfTeam == 0 ? 1 : amplifiedFactor );
        }
        else {
          if ( task.NumberOfTeam != 0 ) {
            var factor = amplifiedFactor - 1;
            var firstStep = stepworks.ElementAt( 0 );
            var gap = firstStep.Duration * factor;
            if ( task.NumberOfTeam > 0 )
              gap /= task.NumberOfTeam;
            for ( int i = 1; i < stepworks.Count(); i++ ) {
              var stepwork = stepworks.ElementAt( i );
              stepwork.Start += gap;
              if ( task.NumberOfTeam > 1 )
                gap += stepwork.Duration * factor;
              else
                gap += stepwork.Duration * factor / task.NumberOfTeam;
            }
          }

          var stepworkResources = new List<StepworkResource>();
          foreach ( var stepwork in stepworks ) {
            if ( stepwork.Portion == 1 ) {
              continue;
            }
            var predecessors = await _predecessorService.GetPredecessorsByStepworkId( stepwork.StepworkId );
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

  [HttpPost( "versions/{versionId}/details" )]
  [Authorize]
  public async Task<IActionResult> SaveProjectDetails( long versionId, [FromBody] ICollection<GroupTaskFormData> formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var version = await _versionService.GetVersionById( versionId );
    if ( version == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    try {
      await _versionService.BatchDeleteVersionDetails( versionId );
    }
    catch ( Exception ex ) {
      return BadRequest( $"Something went wrong: {ex.Message}. {ex.InnerException?.Message}" );
    }

    await SaveProjectTasks( versionId, formData );

    version.ModifiedDate = DateTime.UtcNow;
    await _versionService.UpdateVersion( version );

    return NoContent();
  }

  private async Task SaveProjectTasks( long versionId, ICollection<GroupTaskFormData> formData )
  {
    var setting = await _projectSettingService.GetProjectSetting( versionId );
    var stepworkColor = await _colorService.GetStepworkColorDefsByVersionId( versionId );
    var installColor = stepworkColor.First( c => c.IsInstall == 0 );
    var converter = new ModelConverter( versionId, setting!, installColor!, formData );

    var grouptasks = new Dictionary<string, GroupTask>();
    foreach ( var grouptask in converter.GroupTasks ) {
      var result = await _groupTaskService.CreateGroupTask( grouptask );
      if ( result.Success ) {
        grouptasks.Add( result.Content.LocalId, result.Content );
      }
    }

    var tasks = new Dictionary<string, ModelTask>();
    foreach ( var task in converter.Tasks ) {
      task.GroupTaskId = grouptasks [ task.GroupTaskLocalId ].GroupTaskId;
      var result = await _taskService.CreateTask( task );
      if ( result.Success ) {
        tasks.Add( result.Content.LocalId, result.Content );
      }
    }

    var stepworks = new Dictionary<string, Stepwork>();
    foreach ( var stepwork in converter.Stepworks ) {
      stepwork.TaskId = tasks [ stepwork.TaskLocalId ].TaskId;
      //stepwork.End = stepwork.Start + stepwork.Duration;
      var result = await _stepworkService.CreateStepwork( stepwork );
      if ( result.Success ) {
        stepworks.Add( result.Content.LocalId, result.Content );
      }
    }

    foreach ( var predecessor in converter.Predecessors ) {
      if ( !stepworks.ContainsKey( predecessor.StepworkId ) || !stepworks.ContainsKey( predecessor.RelatedStepworkId ) ) {
        continue;
      }
      await _predecessorService.CreatePredecessor( new Predecessor()
      {
        StepworkId = stepworks [ predecessor.StepworkId ].StepworkId,
        RelatedStepworkId = stepworks [ predecessor.RelatedStepworkId ].StepworkId,
        RelatedStepworkLocalId = predecessor.RelatedStepworkId,
        Type = predecessor.Type,
        Lag = predecessor.Lag
      } );
    }
  }

  [HttpPost( "versions/{versionId}/import" ), DisableRequestSizeLimit]
  [Authorize]
  public async Task<IActionResult> Import( long versionId )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var colors = await _colorService.GetStepworkColorDefsByVersionId( versionId );
    var installColor = colors.FirstOrDefault( c => c.IsInstall == 0 );
    var removalColor = colors.FirstOrDefault( c => c.IsInstall == 1 );

    var formCollection = await Request.ReadFormAsync();
    var file = formCollection.Files.First();

    if ( file.Length <= 0 ) {
      return BadRequest( "No file found." );
    }

    if ( !formCollection.ContainsKey( "SheetName" ) ) {
      return BadRequest( "No sheet name found." );
    }

    if ( !formCollection.ContainsKey( "DisplayOrder" ) ) {
      return BadRequest( "Display Order field missing." );
    }

    if ( !int.TryParse( formCollection [ "DisplayOrder" ].ToString(), out var maxDisplayOrder ) ) {
      return BadRequest( "Display Order is not a number." );
    }

    var sheetNameList = formCollection [ "SheetName" ].ToString().Split( "," );
    var setting = await _projectSettingService.GetProjectSetting( versionId );
    var result = ImportFileUtils.ReadFromFile(
      file.OpenReadStream(),
      sheetNameList,
      setting!,
      installColor!.ColorId,
      removalColor!.ColorId,
      maxDisplayOrder,
      out var messages );

    return Ok( new { Messages = messages, Result = result } );
  }

  [HttpPost( "projects/{projectId}/versions/{versionId}/duplicate" )]
  [Authorize]
  public async Task<IActionResult> DuplicateVersion( long projectId, long versionId )
  {
    try {
      var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
      var version = await _versionService.GetVersionById( versionId );
      if ( version == null ) {
        return BadRequest( ProjectNotification.NonExisted );
      }
      var versions = await _versionService.GetActiveVersions( userId, projectId );

      var versionName = $"Copy of {version.VersionName}";

      var latestName = versions.Select( version => version.VersionName )
        .Where( name => Regex.IsMatch( name, $"{versionName} \\([1-9]\\)" ) )
        .OrderByDescending( x => x ).FirstOrDefault();

      if ( latestName != null ) {
        var duplicatedNumber = latestName.Replace( $"{versionName} ", "" ).Replace( "(", "" ).Replace( ")", "" );
        int.TryParse( duplicatedNumber, out var digit );
        versionName += $" ({digit + 1})";
      }
      else {
        versionName += " (1)";
      }

      var newVersion = new Version()
      {
        VersionName = versionName,
        UserId = version.UserId,
        CreatedDate = DateTime.UtcNow,
        ModifiedDate = DateTime.UtcNow,
        IsActivated = version.IsActivated,
        NumberOfMonths = version.NumberOfMonths
      };

      var result = await _versionService.CreateVersion( projectId, newVersion );
      if ( !result.Success ) {
        return BadRequest( ProjectNotification.ErrorDuplicating );
      }
      var resource = _mapper.Map<VersionResource>( result.Content );
      resource.CreatedDate = resource.CreatedDate.ToLocalTime();
      resource.ModifiedDate = resource.ModifiedDate.ToLocalTime();

      var setting = await _projectSettingService.GetProjectSetting( versionId );
      var newSetting = await _projectSettingService.GetProjectSetting( result.Content.VersionId );
      newSetting!.ColumnWidth = setting!.ColumnWidth;
      newSetting.SeparateGroupTask = setting.SeparateGroupTask;
      newSetting.AssemblyDurationRatio = setting.AssemblyDurationRatio;
      newSetting.RemovalDurationRatio = setting.RemovalDurationRatio;
      newSetting.ColumnWidth = setting.ColumnWidth;
      newSetting.AmplifiedFactor = setting.AmplifiedFactor;
      if ( newSetting != null ) {
        await _projectSettingService.UpdateProjectSetting( newSetting );
      }

      #region Duplicate color
      //await _colorService.DuplicateColorDefs( projectId, result.Content.ProjectId );
      var bgColorList = new Dictionary<long, ColorDef>();
      var swColorList = new Dictionary<long, ColorDef>();

      var swColors = await _colorService.GetStepworkColorDefsByVersionId( versionId );
      var newSwColors = await _colorService.GetStepworkColorDefsByVersionId( result.Content.VersionId );
      foreach ( var color in swColors ) {
        if ( color.IsDefault ) {
          if ( color.IsInstall == 0 || color.IsInstall == 1 ) {
            var newDefColor = newSwColors.FirstOrDefault( x => x.IsInstall == color.IsInstall );
            newDefColor!.Code = color.Code;
            await _colorService.UpdateColorDef( newDefColor );
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
        var newColorResult = await _colorService.CreateColorDef( newColor );
        swColorList.Add( color.ColorId, newColorResult.Content );
      }
      var bgColors = await _colorService.GetBackgroundColorDefsByVersionId( versionId );
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
        var newColorResult = await _colorService.CreateColorDef( newColor );
        bgColorList.Add( color.ColorId, newColorResult.Content );
      }

      var backgrounds = await _backgroundService.GetBackgroundsByVersionId( versionId );
      foreach ( var bg in backgrounds ) {
        var updatingBackground = await _backgroundService.GetProjectBackground( result.Content.VersionId, bg.Month );
        if ( updatingBackground == null ) {
          continue;
        }
        if ( bg.ColorId == null ) {
          continue;
        }
        updatingBackground.ColorId = bgColorList [ bg.ColorId.Value ].ColorId;
        await _backgroundService.UpdateProjectBackground( bg );
      }
      #endregion

      #region Duplicate details
      var predecessorList = new List<Predecessor>();
      var stepworkIdList = new Dictionary<long, long>();
      var groupTasks = await _groupTaskService.GetGroupTasksByVersionId( versionId );
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
        var newGroupTaskResult = await _groupTaskService.CreateGroupTask( newGroupTask );
        if ( !newGroupTaskResult.Success ) {
          continue;
        }
        var tasks = await _taskService.GetTasksByGroupTaskId( groupTask.GroupTaskId );

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

          var newTaskResult = await _taskService.CreateTask( newTask );
          if ( !newTaskResult.Success ) {
            continue;
          }
          var stepworks = await _stepworkService.GetStepworksByTaskId( task.TaskId );

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
              Type = stepwork.Type
            };
            var newStepworkResult = await _stepworkService.CreateStepwork( newStepwork );
            if ( !newStepworkResult.Success ) {
              continue;
            }
            stepworkIdList.Add( stepwork.StepworkId, newStepworkResult.Content.StepworkId );
            var predecessors = await _predecessorService.GetPredecessorsByStepworkId( stepwork.StepworkId );
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
        await _predecessorService.CreatePredecessor( predecessor );
      }
      #endregion

      #region Duplicate view
      var views = await _viewService.GetViewsByProjectId( versionId );
      foreach ( var view in views ) {
        var newView = new View()
        {
          ViewName = view.ViewName,
          VersionId = result.Content.VersionId
        };
        var newViewResult = await _viewService.CreateView( newView );
        if ( newViewResult == null ) {
          continue;
        }
        var viewtasks = await _viewTaskService.GetViewTasksByViewId( view.ViewId );
        foreach ( var task in viewtasks ) {
          var viewtaskFormData = viewtasks.Select( vt => new ViewTaskFormData()
          {
            Id = task.LocalTaskId,
            Group = task.Group,
            DisplayOrder = task.DisplayOrder
          } ).ToList();
          await _viewTaskService.CreateViewTasks( newViewResult.Content.ViewId, viewtaskFormData );
        }
      }
      #endregion

      return Ok( resource );
    }
    catch ( Exception ex ) {
      return BadRequest( $"Something went wrong: {ex.Message}. {ex.InnerException?.Message}" );
    }
  }
}
