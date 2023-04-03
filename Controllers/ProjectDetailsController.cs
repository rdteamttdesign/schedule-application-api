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
using SchedulingTool.Api.Resources.FormBody.projectdetail;
using SchedulingTool.Api.Resources.projectdetail;
using SchedulingTool.Api.Services;
using System.Net.Http.Headers;
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
  private readonly IBackgroundService _backgroundService;

  public ProjectDetailsController(
    IMapper mapper,
    IProjectService projectService,
    IGroupTaskService groupTaskService,
    ITaskService taskService,
    IStepworkService stepworkService,
    IPredecessorService predecessorService,
    IColorDefService colorService,
    IBackgroundService backgroundService )
  {
    _mapper = mapper;
    _projectService = projectService;
    _groupTaskService = groupTaskService;
    _taskService = taskService;
    _stepworkService = stepworkService;
    _predecessorService = predecessorService;
    _colorService = colorService;
    _backgroundService = backgroundService;
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
      var groupTaskResources = await GetGroupTasksByProjectId2( projectId );
      return Ok( groupTaskResources );
    }
    catch ( Exception ex ) {
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

  private async Task<List<object>> GetGroupTasksByProjectId2( long projectId )
  {
    var result = new List<object>();
    var groupTasks = await _groupTaskService.GetGroupTasksByProjectId( projectId );
    var groupTaskResources = _mapper.Map<List<GroupTaskResource>>( groupTasks );
    result.AddRange( groupTaskResources );

    var stepworkColors = ( await _colorService.GetStepworkColorDefsByProjectId( projectId ) ).ToDictionary( x => x.ColorId, x => x.Code );

    foreach ( var groupTask in groupTasks ) {
      var tasks = await _taskService.GetTasksByGroupTaskId( groupTask.GroupTaskId );
      foreach ( var task in tasks ) {

        var stepworks = await _stepworkService.GetStepworksByTaskId( task.TaskId );
        var taskResource = _mapper.Map<TaskResource>( task );

        if ( stepworks.Count() == 1 ) {
          var predecessors = await _predecessorService.GetPredecessorsByStepworkId( stepworks.First().StepworkId );
          var predecessorResources = _mapper.Map<List<PredecessorResource>>( predecessors );
          taskResource.Predecessors = predecessorResources;
          taskResource.ColorId = stepworks.First().ColorId;
          taskResource.Start = stepworks.First().Start;
        }
        else {
          var stepworkResources = new List<StepworkResource>();
          foreach ( var stepwork in stepworks ) {
            var predecessors = await _predecessorService.GetPredecessorsByStepworkId( stepwork.StepworkId );
            var predecessorResources = _mapper.Map<List<PredecessorResource>>( predecessors );
            var stepworkResource = _mapper.Map<StepworkResource>( stepwork );
            stepworkResource.GroupId = groupTask.LocalId;
            stepworkResource.Predecessors = predecessorResources;
            stepworkResources.Add( stepworkResource );
          }
          taskResource.Stepworks = stepworkResources;
        }
        result.Add( taskResource );
      }
    }
    return result;
  }

  //private async Task<List<object>> GetGroupTasksByProjectId1( long projectId )
  //{
  //  var groupTasks = await _groupTaskService.GetGroupTasksByProjectId( projectId );
  //  var groupTaskResources = _mapper.Map<List<GroupTaskDetailResource>>( groupTasks );

  //  var stepworkColors = ( await _colorService.GetStepworkColorDefsByProjectId( projectId ) ).ToDictionary( x => x.ColorId, x => x.Code );

  //  foreach ( var groupTaskResource in groupTaskResources ) {
  //    var tasks = await _taskService.GetTasksByGroupTaskId( groupTaskResource.GroupTaskId );
  //    groupTaskResource.Tasks = _mapper.Map<List<TaskDetailResource>>( tasks );
  //    foreach ( var taskResource in groupTaskResource.Tasks ) {
  //      var stepworks = await _stepworkService.GetStepworksByTaskId( taskResource.TaskId );
  //      taskResource.Stepworks = _mapper.Map<List<StepworkDetailResource>>( stepworks );
  //      foreach ( var stepworkResource in taskResource.Stepworks ) {
  //        var predecessor = await _predecessorService.GetPredecessorsByStepworkId( stepworkResource.StepworkId );
  //        stepworkResource.Predecessors = _mapper.Map<List<PredecessorDetailResource>>( predecessor );
  //        if ( stepworkColors.ContainsKey( stepworkResource.ColorId ) ) {
  //          stepworkResource.ColorCode = stepworkColors [ stepworkResource.ColorId ];
  //        }
  //      }
  //    }
  //  }
  //  return null;
  //}

  [HttpPost( "{projectId}/details" )]
  [Authorize]
  public async Task<IActionResult> SaveProjectDetails( long projectId, [FromBody] ICollection<GroupTaskFormData> formData )
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

    var converter = new ModelConverter( projectId, formData );
    var a = converter.Tasks;

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
      var result = await _stepworkService.CreateStepwork( stepwork );
      if ( result.Success ) {
        stepworks.Add( result.Content.LocalId, result.Content );
      }
    }

    foreach ( var predecessor in converter.Predecessors ) {
      await _predecessorService.CreatePredecessor( new Predecessor()
      {
        StepworkId = stepworks [ predecessor.StepworkId ].StepworkId,
        RelatedStepworkId = stepworks [ predecessor.RelatedStepworkId ].StepworkId,
        RelatedStepworkLocalId = predecessor.RelatedStepworkId,
        Type = predecessor.Type,
        Lag = predecessor.Lag
      } );
    }

    return NoContent();
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

  private async Task<IEnumerable<ProjectBackgroundResource>> GetBackgrounds( long projectId )
  {
    var colors = await _colorService.GetBackgroundColorDefsByProjectId( projectId );
    //var resources = _mapper.Map<IEnumerable<BackgroundColorResource>>( backgroundColors ).ToList();

    var backgrounds = await _backgroundService.GetBackgroundsByProjectId( projectId );
    var resources = _mapper.Map<IEnumerable<ProjectBackgroundResource>>( backgrounds );
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
    return resources;
  } 

  [HttpGet( "{projectId}/download" )]
  //[Authorize]
  public async Task<IActionResult> DownloadFile(long projectId )
  {
    var groupTaskResources = await GetGroupTasksByProjectId( projectId );
    var bgResources = await GetBackgrounds( projectId );
    if ( ExportExcel.ExportExcel.GetFile( groupTaskResources, bgResources, out var result ) ) {
      var fileBytes = System.IO.File.ReadAllBytes( result );
      return File( fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, $"{Guid.NewGuid().ToString()}.xlsx" );
    }
    else {
      return BadRequest( result );
    }
  }
}
