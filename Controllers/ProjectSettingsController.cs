using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources.FormBody;
using SchedulingTool.Api.Resources;

namespace SchedulingTool.Api.Controllers;

[Route( "api" )]
[ApiController]
public class ProjectSettingsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IProjectSettingService _projectSettingService;

  public ProjectSettingsController( IMapper mapper, IProjectSettingService projectSettingService )
  {
    _mapper = mapper;
    _projectSettingService = projectSettingService;
  }

  [HttpGet( "projects/{projectId}/settings" )]
  [Authorize]
  public async Task<IActionResult> GetProjectSetting( long projectId )
  {
    var setting = await _projectSettingService.GetProjectSetting( projectId );
    if ( setting == null ) {
      return BadRequest( ProjectSettingNotification.NonExisted );
    }
    var resource = _mapper.Map<ProjectSettingResource>( setting );
    return Ok( resource );
  }

  [HttpPut( "projects/{projectId}/settings" )]
  [Authorize]
  public async Task<IActionResult> UpdateProjectSetting( long projectId, [FromBody] ProjectSettingFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }

    if ( formData.AssemblyDurationRatio + formData.RemovalDurationRatio != 1 ) {
      return BadRequest( "Total ratio must equal to 1" );
    }

    var setting = await _projectSettingService.GetProjectSetting( projectId );
    if ( setting is null )
      return BadRequest( ProjectSettingNotification.NonExisted );

    setting.SeparateGroupTask = formData.SeparateGroupTask;
    setting.AssemblyDurationRatio = formData.AssemblyDurationRatio;
    setting.RemovalDurationRatio = formData.RemovalDurationRatio;

    var result = await _projectSettingService.UpdateProjectSetting( setting );
    if ( !result.Success )
      return BadRequest( result.Message );

    var resource = _mapper.Map<ProjectSetting>( result.Content );
    return Ok( resource );
  }
}
