using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
  private readonly IVersionService _versionService;

  public ViewsController(
    IMapper mapper,
    IViewService viewService,
    IViewTaskService viewTaskService,
    IVersionService versionService )
  {
    _mapper = mapper;
    _viewService = viewService;
    _viewTaskService = viewTaskService;
    _versionService = versionService;
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
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var userName = HttpContextExtension.GetUserName( HttpContext );
    var version = await _versionService.GetVersionById( versionId );
    if ( version == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }

    var existViewName = await _viewService.IsViewNameExists( versionId, formData.ViewName );
    if ( existViewName ) {
      Response.StatusCode = 409;
      return Ok( new
      {
        Message = $"{formData.ViewName} already existed.",
      } );
    }
    var result = await _viewService.CreateView( versionId, formData );
    if ( !result.Success ) {
      return BadRequest( result.Message );
    }
    version.ModifiedDate = DateTime.UtcNow;
    version.ModifiedBy = userName;
    await _versionService.UpdateVersion( version );

    return Ok( result.Content );
  }

  [HttpPut( "versions/{versionId}/views/{viewId}" )]
  [Authorize]
  public async Task<IActionResult> UpdateView( long versionId, long viewId, [FromBody] ViewFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var userName = HttpContextExtension.GetUserName( HttpContext );
    var version = await _versionService.GetVersionById( versionId );
    if ( version == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    var existViewName = await _viewService.IsViewNameExists( versionId, formData.ViewName, viewId );
    if ( existViewName ) {
      Response.StatusCode = 409;
      return Ok( new
      {
        Message = $"{formData.ViewName} already existed.",
      } );
    }
    var result = await _viewService.UpdateView( versionId, viewId, formData );
    if ( !result.Success ) {
      return BadRequest( result.Message );
    }
    version.ModifiedDate = DateTime.UtcNow;
    version.ModifiedBy = userName;
    await _versionService.UpdateVersion( version );

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

  [HttpPut( "versions/{versionId}/views/{viewId}/details" )]
  [Authorize]
  public async Task<IActionResult> SaveViewDetail( long versionId, long viewId, [FromBody] ICollection<ViewTaskDetailFormData> formData )
  {
    try {
      if ( !ModelState.IsValid ) {
        return BadRequest( ModelState.GetErrorMessages() );
      }
      var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
      var userName = HttpContextExtension.GetUserName( HttpContext );
      var version = await _versionService.GetVersionById( versionId );
      if ( version == null ) {
        return BadRequest( ProjectNotification.NonExisted );
      }
      var result = await _viewService.SaveViewDetail( viewId, formData );
      if ( !result.Success ) {
        return BadRequest( result.Message );
      }

      version.ModifiedDate = DateTime.UtcNow;
      version.ModifiedBy = userName;
      await _versionService.UpdateVersion( version );

      return NoContent();
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ex.Message} {ex.StackTrace}" );
    }
  }
}