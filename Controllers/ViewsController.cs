using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;

namespace SchedulingTool.Api.Controllers;

[Route( "api" )]
[ApiController]
public class ViewsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IViewService _viewService;
  private readonly IViewTaskService _viewTaskService;
  private readonly ITaskService _taskService;
  private readonly IGroupTaskService _groupTaskService;
  private readonly IStepworkService _stepworkService;

  public ViewsController(
    IMapper mapper,
    IViewService viewService,
    IViewTaskService viewTaskService,
    ITaskService taskService,
    IGroupTaskService groupTaskService,
    IStepworkService stepworkService )
  {
    _mapper = mapper;
    _viewService = viewService;
    _viewTaskService = viewTaskService;
    _taskService = taskService;
    _groupTaskService = groupTaskService;
    _stepworkService = stepworkService;
  }

  [HttpGet( "projects/{projectId}/views" )]
  [Authorize]
  public async Task<IActionResult> GetViewsInProject( long projectId )
  {
    var views = await _viewService.GetViewsByProjectId( projectId );
    var viewResources = _mapper.Map<IEnumerable<ViewResource>>( views );
    foreach ( var viewResource in viewResources ) {
      var viewTasks = await _viewTaskService.GetViewTasksByViewId( viewResource.ViewId );
      viewResource.ViewTasks = _mapper.Map<IEnumerable<ViewTaskResource>>( viewTasks ).OrderBy( t => t.DisplayOrder ).ToList();
    }
    return Ok( viewResources );
  }

  [HttpPost( "projects/{projectId}/views" )]
  [Authorize]
  public async Task<IActionResult> CreateView( long projectId, [FromBody] ViewFormData formData )
  {
    // checking
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    // create view
    var view = new View()
    {
      ViewName = formData.ViewName,
      ProjectId = projectId
    };
    var result = await _viewService.CreateView( view );
    if ( !result.Success ) {
      return BadRequest( result.Message );
    }
    var resource = _mapper.Map<ViewResource>( result.Content );
    // create tasks include view
    var _ = await _viewTaskService.CreateViewTasks( result.Content.ViewId, formData.Tasks );
    if ( _.Success )
      resource.ViewTasks = _.Content;

    return Ok( resource );
  }

  [HttpPut( "views/{viewId}" )]
  [Authorize]
  public async Task<IActionResult> UpdateView( long viewId, [FromBody] ViewFormData formData )
  {
    try {
      // checking
      var view = await _viewService.GetViewById( viewId );
      if ( view == null ) {
        return BadRequest( ViewNotification.NonExisted );
      }
      view.ViewName = formData.ViewName;
      await _viewService.UpdateView( view );
      // clear tasks in view
      await _viewService.DeleteView( viewId, false );
      // create tasks include view
      await _viewTaskService.CreateViewTasks( viewId, formData.Tasks );
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ViewNotification.ErrorSaving} {ex.Message}" );
    }
    return NoContent();
  }

  [HttpDelete( "views/{viewId}" )]
  [Authorize]
  public async Task<IActionResult> DeleteView( long viewId )
  {
    try {
      await _viewService.DeleteView( viewId, true );
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ViewNotification.ErrorDeleting} {ex.Message}" );
    }
    return NoContent();
  }

  [HttpGet( "projects/{projectId}/views/{viewId}" )]
  [Authorize]
  public async Task<IActionResult> GetViewDetail( long projectId, long viewId )
  {
    // checking
    var view = await _viewService.GetViewById( viewId );
    if ( view == null ) {
      return BadRequest( ViewNotification.NonExisted );
    }
    // get tasks in view
    var viewTasks = await _viewService.GetViewTasks( projectId, viewId );

    if ( !viewTasks.Any() ) {
      return BadRequest( "View has no task." );
    }

    foreach ( var viewTask in viewTasks ) {
      var stepworks = await _stepworkService.GetStepworksByTaskId( viewTask.TaskId );
      viewTask.Stepworks = stepworks.ToList();
    }

    var viewDetail = await _viewService.GetViewDetailById( projectId, viewTasks.OrderBy( t => t.DisplayOrder ) );
    return Ok( viewDetail );
  }
}