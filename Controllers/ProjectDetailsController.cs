using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Extension;
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
    return Ok();
  }

  [HttpPost( "{projectId}/details" )]
  [Authorize]
  public async Task<IActionResult> SaveProjectDetails( long projectId, [FromBody] ProjectDetailFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    foreach ( var groupTaskData in formData.GroupTasks ) {
      var groupTask = new GroupTask() {
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
      stepworkData.StepworkId = result.Content.StepWorkId;
    }
    return NoContent();
  }
}
