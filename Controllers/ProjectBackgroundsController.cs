using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;

namespace SchedulingTool.Api.Controllers;

[Route( "api" )]
[ApiController]
public class ProjectBackgroundsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IBackgroundService _backgroundService;
  private readonly IVersionService _projectService;

  public ProjectBackgroundsController(
    IMapper mapper,
    IBackgroundService backgroundService,
    IVersionService projectService )
  {
    _mapper = mapper;
    _backgroundService = backgroundService;
    _projectService = projectService;
  }

  [HttpGet( "projects/{projectId}/backgrounds" )]
  [Authorize]
  public async Task<IActionResult> GetProjectBackgrounds( long projectId )
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var project = await _projectService.GetVersion( userId, projectId );
    if ( project == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    var backgrounds = await _backgroundService.GetBackgroundsByProjectId( projectId );
    if ( backgrounds == null ) {
      return BadRequest( BackgroundNotification.NonExisted );
    }
    var resource = _mapper.Map<IEnumerable<BackgroundResource>>( backgrounds );
    return Ok( resource );
  }

  [HttpDelete( "projects/{projectId}/backgrounds" )]
  [Authorize]
  public async Task<IActionResult> DeleteProjectBackgroundsFromMonth( long projectId, [FromBody] DeleteBackgroundsFormBody formBody )
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var project = await _projectService.GetVersion( userId, projectId );
    if ( project == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    try {
      _backgroundService.BatchDelete( projectId, formBody.DeleteFromMonth );
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ex.Message}: {ex.StackTrace}" );
    }
    return NoContent();
  }

  [HttpPut( "projects/{projectId}/backgrounds" )]
  [Authorize]
  public async Task<IActionResult> UpdateProjectBackgroundsFromMonth( long projectId, [FromBody] ICollection<BackgroundFormBody> formBody )
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var project = await _projectService.GetVersion( userId, projectId );
    if ( project == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    try {
      foreach ( var item in formBody ) {
        var projectBackground = await _backgroundService.GetProjectBackground( projectId, item.Month );
        if ( projectBackground == null )
          continue;
        projectBackground.ColorId = item.ColorId;
        await _backgroundService.UpdateProjectBackground( projectBackground );
      }
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ex.Message}: {ex.StackTrace}" );
    }
    return NoContent();
  }
}
