using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Models.Enum;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Extension;
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
  public async Task<IActionResult> SaveProjectDetails( long projectId, [FromBody] ICollection<GroupTaskDetailFormData> groupTaskFormData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }

    foreach ( var groupTask in groupTaskFormData ) {
      foreach ( var task in groupTask.Tasks ) {
        foreach ( var stepwork in task.Stepworks ) {
          foreach ( var predecessor in stepwork.Predecessors ) {
            UpdatePredecessors( stepwork.StepworkId, predecessor );
          }
        }
      }
    }

    foreach ( var groupTask in groupTaskFormData ) {
      foreach ( var task in groupTask.Tasks ) {
        foreach ( var stepwork in task.Stepworks ) {
          UpdateStepworks( task.TaskId, stepwork );
        }
      }
    }

    foreach ( var groupTask in groupTaskFormData ) {
      foreach ( var task in groupTask.Tasks ) {
        UpdateTasks( groupTask.GroupTaskId, task );
      }
    }

    foreach ( var groupTask in groupTaskFormData ) {
      UpdateGroupTasks( projectId, groupTask );
    }

    return Ok();
  }

  private async void UpdateGroupTasks( long projectId, GroupTaskDetailFormData formData )
  {
    switch ( formData.Change ) {
      case DataChange.Create:
        var groupTask = new GroupTask()
        {
          GroupTaskName = formData.GroupTaskName,
          ProjectId = projectId
        };
        await _groupTaskService.CreateGroupTask( groupTask );
        break;

      case DataChange.Update:
        var existingGroupTask = await _groupTaskService.GetGroupTaskById( formData.GroupTaskId );
        if ( existingGroupTask == null )
          break;
        existingGroupTask.GroupTaskName = formData.GroupTaskName;
        await _groupTaskService.UpdateGroupTask( existingGroupTask );
        break;

      case DataChange.Delete:
        _groupTaskService.DeleteGroupTask( formData.GroupTaskId );
        break;

      default:
        break;
    }
  }

  private async Task UpdateTasks( long groupTaskId, TaskDetailFormData formData )
  {
    switch ( formData.Change ) {
      case DataChange.Create:
        var task = new ModelTask()
        {
          TaskName = formData.TaskName,
          Index = formData.Index,
          NumberOfTeam = formData.NumberOfTeam,
          Duration = formData.Duration,
          AmplifiedDuration = formData.AmplifiedDuration,
          GroupTaskId = groupTaskId,
          Description = formData.Description,
          Note = formData.Note
        };
        await _taskService.CreateTask( task );
        break;

      case DataChange.Update:
        var existingTask = await _taskService.GetTaskById( formData.TaskId );
        if ( existingTask == null )
          break;
        existingTask.TaskName = formData.TaskName;
        existingTask.Index = formData.Index;
        existingTask.NumberOfTeam = formData.NumberOfTeam;
        existingTask.Duration = formData.Duration;
        existingTask.AmplifiedDuration = formData.AmplifiedDuration;
        existingTask.GroupTaskId = groupTaskId;
        existingTask.Description = formData.Description;
        existingTask.Note = formData.Note;
        await _taskService.UpdateTask( existingTask );
        break;

      case DataChange.Delete:
        _taskService.DeleteTask( formData.TaskId );
        break;

      default:
        break;
    }
  }

  private async void UpdateStepworks( long taskId, StepworkDetailFormData formData )
  {
    switch ( formData.Change ) {
      case DataChange.Create:
        var stepwork = new Stepwork()
        {
          Duration = formData.Duration,
          TaskId = taskId,
          ColorId = formData.ColorId
        };
        await _stepworkService.CreateStepwork( stepwork );
        break;

      case DataChange.Update:
        var existingStepwork = await _stepworkService.GetStepworkById( formData.StepworkId );
        if ( existingStepwork == null )
          break;
        existingStepwork.Duration = formData.Duration;
        existingStepwork.ColorId = formData.ColorId;
        await _stepworkService.UpdateStepwork( existingStepwork );
        break;

      case DataChange.Delete:
        _stepworkService.DeleteStepwork( formData.StepworkId );
        break;

      default:
        break;
    }
  }

  private async void UpdatePredecessors( long stepworkId, PredecessorDetailFormData formData )
  {
    switch ( formData.Change ) {
      case DataChange.Create:
        var predecessor = new Predecessor()
        {
          StepworkId = stepworkId,
          RelatedStepworkId = formData.RelatedStepworkId,
          Lag = formData.Lag,
          Type = formData.Type
        };
        await _predecessorService.CreatePredecessor( predecessor );
        break;

      case DataChange.Update:
        var existingPredecessor = await _predecessorService.GetPredecessor( stepworkId, formData.RelatedStepworkId );
        if ( existingPredecessor == null )
          break;
        existingPredecessor.RelatedStepworkId = formData.RelatedStepworkId;
        existingPredecessor.Lag = formData.Lag;
        existingPredecessor.Type = formData.Type;
        await _predecessorService.UpdatePredecessor( existingPredecessor );
        break;

      case DataChange.Delete:
        _predecessorService.DeletePredecessor( stepworkId, formData.RelatedStepworkId );
        break;

      default:
        break;
    }
  }
}
