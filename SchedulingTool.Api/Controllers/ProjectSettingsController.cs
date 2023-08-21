using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Resources.FormBody;

namespace SchedulingTool.Api.Controllers;

[Route( "api" )]
[ApiController]
public class ProjectSettingsController : ControllerBase
{
  private readonly IProjectSettingService _projectSettingService;

  public ProjectSettingsController( IProjectSettingService projectSettingService )
  {
    _projectSettingService = projectSettingService;
  }

  [HttpGet( "versions/{versionId}/settings" )]
  [Authorize]
  public async Task<IActionResult> GetVersionSetting( long versionId )
  {
    try {
      var result = await _projectSettingService.GetProjectSettingByVersionId( versionId );
      if ( result.Success ) {
        return Ok( result.Content );
      }
      else {
        return BadRequest( result.Message );
      }
    }
    catch ( Exception ex ) {
      return BadRequest( $"Some thing went wrong.\n\n{ex.Message} {ex.StackTrace}" );
    }

  }

  [HttpPut( "versions/{versionId}/settings" )]
  [Authorize]
  public async Task<IActionResult> UpdateProjectSetting( long versionId, [FromBody] ProjectSettingFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }

    if ( formData.AssemblyDurationRatio + formData.RemovalDurationRatio != 1 ) {
      return BadRequest( "Total ratio must equal to 1" );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var userName = HttpContextExtension.GetUserName( HttpContext );
    var result = await _projectSettingService.UpdateProjectSettingByVersionId( versionId, userName, formData );

    if ( result.Success ) {
      return NoContent();
    }
    else {
      return BadRequest( result.Message );
    }
  }

  [HttpPost( "settings/calculate-date-ranges" )]
  [Authorize]
  public IActionResult CalculateDateRanges( [FromBody] CalculateDateRangesFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var result = _projectSettingService.CalculateDateRanges( formData );
    if ( result.Success ) {
      return Ok( result.Content );
    }
    else {
      return BadRequest( result.Message );
    }
  }
}
