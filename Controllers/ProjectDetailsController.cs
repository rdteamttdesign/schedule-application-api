using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Controllers;

[Route( "api/projects" )]
[ApiController]
public class ProjectDetailsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IProjectService _projectService;
  private readonly IGroupTaskService _groupTaskService;
  private readonly ITaskService _taskService;
  private readonly IStepworkService _stepworkService;
  private readonly IPredecessorService _predecessorService;
  private readonly IColorDefService _colorService;

  public ProjectDetailsController(
    IMapper mapper,
    IProjectService projectService,
    IGroupTaskService groupTaskService,
    ITaskService taskService,
    IStepworkService stepworkService,
    IPredecessorService predecessorService,
    IColorDefService colorService )
  {
    _mapper = mapper;
    _projectService = projectService;
    _groupTaskService = groupTaskService;
    _taskService = taskService;
    _stepworkService = stepworkService;
    _predecessorService = predecessorService;
    _colorService = colorService;
  }

  [HttpGet( "{projectId}/details" )]
  [Authorize]
  public async Task<IActionResult> GetProjectDetails( long projectId )
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var project = await _projectService.GetProject( userId, projectId );
    if ( project == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    try {
      var groupTaskResources = await GetGroupTasksByProjectId( projectId );
      return Ok( groupTaskResources );
    }
    catch ( Exception ex) {
      return BadRequest( $"Something went wrong: {ex.Message}" );
    }
  }

  private async Task<List<GroupTaskDetailResource>> GetGroupTasksByProjectId( long projectId )
  {
    var groupTasks = await _groupTaskService.GetGroupTasksByProjectId( projectId );
    var groupTaskResources = _mapper.Map<List<GroupTaskDetailResource>>( groupTasks );

    var stepworkColors = ( await _colorService.GetStepworkColorDefsByProjectId( projectId ) ).ToDictionary( x => x.ColorId, x => x.Code );

    foreach ( var groupTaskResource in groupTaskResources ) {
      var tasks = await _taskService.GetTasksByGroupTaskId( groupTaskResource.GroupTaskId );
      groupTaskResource.Tasks = _mapper.Map<List<TaskDetailResource>>( tasks );
      foreach ( var taskResource in groupTaskResource.Tasks ) {
        var stepworks = await _stepworkService.GetStepworksByTaskId( taskResource.TaskId );
        taskResource.Stepworks = _mapper.Map<List<StepworkDetailResource>>( stepworks );
        foreach ( var stepworkResource in taskResource.Stepworks ) {
          var predecessor = await _predecessorService.GetPredecessorsByStepworkId( stepworkResource.StepworkId );
          stepworkResource.Predecessors = _mapper.Map<List<PredecessorDetailResource>>( predecessor );
          if ( stepworkColors.ContainsKey( stepworkResource.ColorId ) ) {
            stepworkResource.ColorCode = stepworkColors [ stepworkResource.ColorId ];
          }
        }
      }
    }
    return groupTaskResources;
  }

  [HttpPost( "{projectId}/details" )]
  [Authorize]
  public async Task<IActionResult> SaveProjectDetails( long projectId, [FromBody] ICollection<GroupTaskDetailFormData> formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var project = await _projectService.GetProject( userId, projectId );
    if ( project == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    try {
      await _projectService.BatchDeleteProjectDetails( projectId );
    }
    catch ( Exception ex ) {
      return BadRequest( ex.Message );
    }

    var stepworks = new Dictionary<string, Stepwork>();
    foreach ( var groupTaskData in formData ) {
      var grouptaskResult = await CreateGrouptask( projectId, groupTaskData );
      if ( !grouptaskResult.Success )
        continue;
      foreach ( var taskData in groupTaskData.Tasks ) {
        var taskResult = await CreateTask( grouptaskResult.Content.GroupTaskId, taskData );
        if ( !taskResult.Success )
          continue;
        foreach ( var stepworkData in taskData.Stepworks ) {
          var stepworkResult = await CreateStepwork( taskResult.Content.TaskId, stepworkData );
          if ( !stepworkResult.Success )
            continue;
          foreach ( var predecessorData in stepworkData.Predecessors ) {
            predecessorData.StepworkId = stepworkResult.Content.StepworkId;
          }
          stepworks.Add( $"{grouptaskResult.Content.Index},{taskResult.Content.Index},{stepworkResult.Content.Index}", stepworkResult.Content );
        }
      }
    }

    foreach ( var groupTaskData in formData ) {
      foreach ( var taskData in groupTaskData.Tasks ) {
        foreach ( var stepworkData in taskData.Stepworks ) {
          foreach ( var predecessorData in stepworkData.Predecessors ) {
            var key = $"{predecessorData.RelatedGroupTaskIndex},{predecessorData.RelatedTaskIndex},{predecessorData.RelatedStepworkIndex}";
            if ( stepworks.ContainsKey( key ) ) {
              continue;
            }
            var relatedStepwork = stepworks [ key ];
            await CreatePredecessor( relatedStepwork.StepworkId, predecessorData );
          }
        }
      }
    }

    return NoContent();
  }

  private async Task<ServiceResponse<GroupTask>> CreateGrouptask( long projectId, GroupTaskDetailFormData groupTaskData )
  {
    var groupTask = new GroupTask()
    {
      GroupTaskName = groupTaskData.GroupTaskName,
      ProjectId = projectId,
      Index = groupTaskData.Index
    };
    return await _groupTaskService.CreateGroupTask( groupTask );
  }

  private async Task<ServiceResponse<ModelTask>> CreateTask( long groupId, TaskDetailFormData taskData )
  {
    var task = new ModelTask()
    {
      TaskName = taskData.TaskName,
      Index = taskData.Index,
      NumberOfTeam = taskData.NumberOfTeam,
      Duration = taskData.Duration,
      AmplifiedDuration = taskData.AmplifiedDuration,
      GroupTaskId = groupId,
      Description = taskData.Description,
      Note = taskData.Note
    };
    return await _taskService.CreateTask( task );
  }

  private async Task<ServiceResponse<Stepwork>> CreateStepwork( long taskId, StepworkDetailFormData stepworkData )
  {
    var stepwork = new Stepwork()
    {
      Index = stepworkData.Index,
      Portion = stepworkData.Portion,
      TaskId = taskId,
      ColorId = stepworkData.ColorId
    };
    return await _stepworkService.CreateStepwork( stepwork );
  }

  private async Task CreatePredecessor( long relatedStepworkId, PredecessorDetailFormData predecessorData )
  {
    if ( predecessorData.StepworkId == null ) {
      return;
    }
    var predecessor = new Predecessor()
    {
      StepworkId = predecessorData.StepworkId.Value,
      RelatedStepworkId = relatedStepworkId,
      Type = predecessorData.Type,
      Lag = predecessorData.Lag
    };
    await _predecessorService.CreatePredecessor( predecessor );
  }

  [HttpPost( "import" ), DisableRequestSizeLimit]
  [Authorize]
  public async Task<IActionResult> Import()
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var formCollection = await Request.ReadFormAsync();
    var file = formCollection.Files.First();
    var sheetNameList = formCollection [ "SheetName" ];
    var result = ImportFileUtils.ReadFromFile( file.OpenReadStream(), sheetNameList );
    return Ok( result );
  }

  //[HttpGet( "{projectId}/export" )]
  //[Authorize]
  //public async Task<IActionResult> Export( long projectId )
  //{
  //  var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
  //  var project = await _projectService.GetProject( userId, projectId );
  //  if ( project == null ) {
  //    return BadRequest( ProjectNotification.NonExisted );
  //  }
  //  var groupTaskResources = await GetGroupTasksByProjectId( projectId );
  //  var fileBytes = ImportFileUtils.WriteToFile( groupTaskResources );
  //  return File( fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, $"{Guid.NewGuid().ToString()}.xlsx" );
  //}

  [HttpGet( "{projectId}/download" )]
  //[Authorize]
  public async Task<IActionResult> DownloadFile(long projectId )
  {
    var groupTaskResources = await GetGroupTasksByProjectId( projectId );
    if ( ExportExcel.ExportExcel.GetFile( groupTaskResources, out var result ) ) {
      var fileBytes = System.IO.File.ReadAllBytes( result );
      return File( fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, $"{Guid.NewGuid().ToString()}.xlsx" );
    }
    else {
      return BadRequest( result );
    }
  }
}
