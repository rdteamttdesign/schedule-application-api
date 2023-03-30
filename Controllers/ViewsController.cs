using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using SchedulingTool.Api.Services;

namespace SchedulingTool.Api.Controllers;

[Route( "api" )]
[ApiController]
public class ViewsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IViewService _viewService;
  private readonly IViewTaskService _viewTaskService;

  public ViewsController(
    IMapper mapper,
    IViewService viewService,
    IViewTaskService viewTaskService )
  {
    _mapper = mapper;
    _viewService = viewService;
    _viewTaskService = viewTaskService;
  }

  [HttpGet( "projects/{projectId}/views" )]
  [Authorize]
  public async Task<IActionResult> GetViewsInProject( long projectId )
  {
    var views = await _viewService.GetViewsByProjectId( projectId );
    var viewResources = _mapper.Map<IEnumerable<ViewResource>>( views );
    if ( !viewResources.Any() ) {
      return BadRequest( ViewNotification.NonExisted );
    }
    foreach ( var viewResource in viewResources ) {
      var viewTasks = await _viewTaskService.GetViewTasksByViewId( viewResource.ViewId );
      if ( viewTasks.Any() ) {
        var viewTaskResources = _mapper.Map<IEnumerable<ViewTaskResource>>( viewTasks );
        viewResource.ViewTasks = viewTaskResources.ToList();
      }
    }
    return Ok( viewResources );
  }

  [HttpPost( "projects/{projectId}/views" )]
  [Authorize]
  public async Task<IActionResult> CreateView( long projectId, [FromBody] ViewFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
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

    foreach ( var item in formData.Tasks ) {
      var viewTask = new ViewTask()
      {
        ViewId = result.Content.ViewId,
        //TaskId = item.TaskId,
        Group = item.Group
      };
      var viewTaskResult = await _viewTaskService.CreateViewTask( viewTask );
      if ( viewTaskResult.Success ) {
        var viewTaskResource = _mapper.Map<ViewTaskResource>( viewTaskResult.Content );
        resource.ViewTasks.Add( viewTaskResource );
      }
    }

    return Ok( resource );
  }

  [HttpPut( "views/{viewId}" )]
  [Authorize]
  public async Task<IActionResult> UpdateView( long viewId, [FromBody] ViewFormData formData )
  {
    try {

      var view = await _viewService.GetViewById( viewId );
      if ( view == null ) {
        return BadRequest( ViewNotification.NonExisted );
      }
      view.ViewName = formData.ViewName;
      await _viewService.UpdateView( view );

      await _viewTaskService.DeleteViewTasksByViewId( viewId );
      foreach ( var item in formData.Tasks ) {
        var viewTask = new ViewTask()
        {
          ViewId = view.ViewId,
          //TaskId = item.TaskId,
          Group = item.Group
        };
        await _viewTaskService.CreateViewTask( viewTask );
      }
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
      await _viewService.DeleteView( viewId );
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ViewNotification.ErrorDeleting} {ex.Message}" );
    }
    return NoContent();
  }
}