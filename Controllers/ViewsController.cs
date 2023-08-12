using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;

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

  [HttpGet( "versions/{versionId}/views" )]
  [Authorize]
  public async Task<IActionResult> GetViewsInProject( long versionId )
  {
    var views = await _viewService.GetViewsByVersionId( versionId );
    var viewResources = _mapper.Map<IEnumerable<ViewResource>>( views );
    foreach ( var viewResource in viewResources ) {
      var viewTasks = await _viewTaskService.GetViewTasksByViewId( viewResource.ViewId );
      viewResource.ViewTasks = _mapper.Map<IEnumerable<ViewTaskResource>>( viewTasks ).OrderBy( t => t.DisplayOrder ).ToList();
    }
    return Ok( viewResources );
  }

  [HttpPost( "versions/{versionId}/views" )]
  [Authorize]
  public async Task<IActionResult> CreateView( long versionId, [FromBody] ViewFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var result = await _viewService.CreateView( versionId, formData );
    if ( !result.Success ) {
      return BadRequest( result.Message );
    }
    return Ok( result.Content );
  }

  [HttpPut( "versions/{versionId}/views/{viewId}" )]
  [Authorize]
  public async Task<IActionResult> UpdateView( long versionId, long viewId, [FromBody] ViewFormData formData )
  {
    var result = await _viewService.UpdateView( versionId, viewId, formData );
    if ( !result.Success ) {
      return BadRequest( result.Message );
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

  [HttpGet( "versions/{versionId}/views/{viewId}" )]
  [Authorize]
  public async Task<IActionResult> GetViewDetail( long versionId, long viewId )
  {
    try {
      var result = await _viewService.GetViewDetailById( versionId, viewId );
      if ( !result.Success ) {
        return BadRequest( result.Message );
      }
      return Ok( result.Content );
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ex.Message} {ex.StackTrace}" );
    }
  }

  [HttpPut( "versions/{versionId}/views/{viewId}" )]
  [Authorize]
  public async Task<IActionResult> SaveViewDetail( long versionId, long viewId, [FromBody] ICollection<ViewTaskDetailFormData> formData )
  {
    try {
      var result = await _viewService.SaveViewDetail( viewId, formData );
      if ( !result.Success ) {
        return BadRequest( result.Message );
      }
      return Ok( result.Content );
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ex.Message} {ex.StackTrace}" );
    }
  }
}