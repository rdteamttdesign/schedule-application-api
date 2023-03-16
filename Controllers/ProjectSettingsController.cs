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
  private readonly IColorDefService _colorDefService;
  private readonly IBackgroundService _backgroundService;
  private readonly IProjectService _projectService;

  public ProjectSettingsController(
    IMapper mapper,
    IProjectSettingService projectSettingService,
    IColorDefService colorDefService,
    IBackgroundService backgroundService,
    IProjectService projectService )
  {
    _mapper = mapper;
    _projectSettingService = projectSettingService;
    _colorDefService = colorDefService;
    _backgroundService = backgroundService;
    _projectService = projectService;
  }

  [HttpGet( "projects/{projectId}/settings" )]
  [Authorize]
  public async Task<IActionResult> GetProjectSetting( long projectId )
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var project = await _projectService.GetProject( userId, projectId );
    if ( project == null ) {
      return BadRequest( ProjectSettingNotification.NonExisted );
    }
    var setting = await _projectSettingService.GetProjectSetting( projectId );
    var resource = _mapper.Map<ProjectSettingResource>( setting );
    resource.NumberOfMonths = project.NumberOfMonths;

    var stepworkColors = await _colorDefService.GetStepworkColorDefsByProjectId( projectId );
    resource.StepworkColors = _mapper.Map<IEnumerable<ColorDefResource>>( stepworkColors ).ToList();

    var backgroundColors = await _colorDefService.GetBackgroundColorDefsByProjectId( projectId );
    resource.BackgroundColors = _mapper.Map<IEnumerable<BackgroundColorResource>>( backgroundColors ).ToList();

    var backgrounds = await _backgroundService.GetBackgroundsByProjectId( projectId );
    var backgroundsGroupBy = backgrounds.Where( bg => bg.ColorId != null ).GroupBy( bg => bg.ColorId )?.ToDictionary( g => g.Key, g => g );
    foreach ( var backgroundColor in resource.BackgroundColors ) {
      if ( backgroundsGroupBy.ContainsKey( backgroundColor.ColorId ) ) {
        backgroundColor.Months = backgroundsGroupBy [ backgroundColor.ColorId ].Select( bg => bg.Month ).ToList();
      }
      else {
        backgroundColor.Months = new List<int>();
      }
        
    }

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
