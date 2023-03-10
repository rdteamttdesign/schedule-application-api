using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;

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

  public ProjectDetailsController(
    IMapper mapper,
    IProjectService projectService,
    IGroupTaskService groupTaskService,
    ITaskService taskService,
    IStepworkService stepworkService,
    IPredecessorService predecessorService )
  {
    _mapper = mapper;
    _projectService = projectService;
    _groupTaskService = groupTaskService;
    _taskService = taskService;
    _stepworkService = stepworkService;
    _predecessorService = predecessorService;
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

    var groupTasks = await _groupTaskService.GetGroupTasksByProjectId( projectId );
    var groupTaskResources = _mapper.Map<List<GroupTaskDetailResource>>( groupTasks );

    foreach ( var groupTaskResource in groupTaskResources ) {
      var tasks = await _taskService.GetTasksByGroupTaskId( groupTaskResource.GroupTaskId );
      groupTaskResource.Tasks = _mapper.Map<List<TaskDetailResource>>( tasks );
      foreach ( var taskResource in groupTaskResource.Tasks ) {
        var stepworks = await _stepworkService.GetStepworksByTaskId( taskResource.TaskId );
        taskResource.Stepworks = _mapper.Map<List<StepworkDetailResource>>( stepworks );
        foreach ( var stepworkResource in taskResource.Stepworks ) {
          var predecessor = await _predecessorService.GetPredecessorsByStepworkId( stepworkResource.StepworkId );
          stepworkResource.Predecessors = _mapper.Map<List<PredecessorDetailResource>>( predecessor );
        }
      }
    }

    return Ok( groupTaskResources );
  }

  [HttpPost( "{projectId}/details" )]
  [Authorize]
  public async Task<IActionResult> SaveProjectDetails( long projectId, [FromBody] ProjectDetailFormData formData )
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

    foreach ( var groupTaskData in formData.GroupTasks ) {
      var groupTask = new GroupTask()
      {
        GroupTaskName = groupTaskData.GroupTaskName,
        ProjectId = projectId,
        Index = groupTaskData.Index
      };
      var result = await _groupTaskService.CreateGroupTask( groupTask );
      if ( !result.Success )
        continue;
      groupTaskData.GroupTaskId = result.Content.GroupTaskId;
    }

    foreach ( var taskData in formData.Tasks ) {
      var group = formData.GroupTasks.FirstOrDefault( gr => gr.Index == taskData.Index );
      if ( group == null ) {
        continue;
      }
      var task = new ModelTask()
      {
        TaskName = taskData.TaskName,
        Index = taskData.Index,
        NumberOfTeam = taskData.NumberOfTeam,
        Duration = taskData.Duration,
        AmplifiedDuration = taskData.AmplifiedDuration,
        GroupTaskId = group.GroupTaskId,
        Description = taskData.Description,
        Note = taskData.Note
      };
      var result = await _taskService.CreateTask( task );
      if ( !result.Success )
        continue;
      taskData.TaskId = result.Content.TaskId;
    }

    foreach ( var stepworkData in formData.Stepworks ) {
      var task = formData.Tasks.FirstOrDefault( task => task.Index == stepworkData.TaskIndex && task.GroupIndex == stepworkData.GroupTaskIndex );
      if ( task == null ) {
        continue;
      }
      var stepwork = new Stepwork()
      {
        Index = stepworkData.Index,
        Duration = stepworkData.Duration,
        TaskId = task.TaskId,
        ColorId = stepworkData.ColorId
      };
      var result = await _stepworkService.CreateStepwork( stepwork );
      if ( !result.Success )
        continue;
      stepworkData.StepworkId = result.Content.StepworkId;
    }

    foreach ( var stepworkData in formData.Stepworks ) {
      foreach ( var predecessorData in stepworkData.Predecessors ) {
        var stepwork = formData.Stepworks.FirstOrDefault(
          sw => sw.Index == predecessorData.RelatedStepworkIndex
          && sw.TaskIndex == predecessorData.RelatedTaskIndex
          && sw.GroupTaskIndex == predecessorData.RelatedGroupTaskIndex );
        if ( stepwork == null ) {
          continue;
        }
        var predecessor = new Predecessor()
        {
          StepworkId = stepworkData.StepworkId,
          RelatedStepworkId = stepwork.StepworkId,
          Type = predecessorData.Type,
          Lag = predecessorData.Lag
        };
        await _predecessorService.CreatePredecessor( predecessor );
      }
    }

    return NoContent();
  }
}
