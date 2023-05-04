using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
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
using SchedulingTool.Api.Services;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Controllers;

[Route( "api/[controller]" )]
[ApiController]
public class ProjectsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IProjectSettingService _projectSettingService;
  private readonly IGroupTaskService _groupTaskService;
  private readonly ITaskService _taskService;
  private readonly IStepworkService _stepworkService;
  private readonly IPredecessorService _predecessorService;
  private readonly IColorDefService _colorService;
  private readonly IBackgroundService _backgroundService;
  private readonly IViewService _viewService;

  public ProjectsController(
    IMapper mapper,
    IProjectSettingService projectSettingService,
    IGroupTaskService groupTaskService,
    ITaskService taskService,
    IStepworkService stepworkService,
    IPredecessorService predecessorService,
    IColorDefService colorService,
    IBackgroundService backgroundService,
    IViewService viewService )
  {
    _mapper = mapper;
    _groupTaskService = groupTaskService;
    _taskService = taskService;
    _stepworkService = stepworkService;
    _predecessorService = predecessorService;
    _colorService = colorService;
    _projectSettingService = projectSettingService;
    _backgroundService = backgroundService;
    _viewService = viewService;
  }

  [HttpGet( "{projectId}/download" )]
  //[Authorize]
  public async Task<IActionResult> DownloadFile( long projectId )
  {
    var groupTaskResources = await GetGroupTaskResourcesByProjectId( projectId );
    var bgResources = await GetBackgrounds( projectId );
    var setting = await _projectSettingService.GetProjectSetting( projectId );
    var viewResources = await GetViewTasks( projectId );
    if ( ExportExcel.ExportExcel.GetFile( setting!, viewResources, groupTaskResources, bgResources, out var result ) ) {
      var fileBytes = System.IO.File.ReadAllBytes( result );
      return File( fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, $"{Guid.NewGuid()}.xlsx" );
    }
    else {
      return BadRequest( result );
    }
  }

  private async Task<Dictionary<View, List<ViewTaskDetail>>> GetViewTasks( long projectId )
  {
    var result = new Dictionary<View, List<ViewTaskDetail>>();
    var views = await _viewService.GetViewsByProjectId( projectId );
    foreach ( var view in views ) {
      var viewTasks = await _viewService.GetViewTasks( projectId, view.ViewId );
      result.Add( view, viewTasks.ToList() );
    }
    return result;
  }

  private async Task<List<GroupTaskDetailResource>> GetGroupTaskResourcesByProjectId( long projectId )
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

  private async Task<IEnumerable<ProjectBackgroundResource>> GetBackgrounds( long projectId )
  {
    var colors = await _colorService.GetBackgroundColorDefsByProjectId( projectId );

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
}
