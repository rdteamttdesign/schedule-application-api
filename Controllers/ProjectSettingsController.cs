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
        var months = backgroundsGroupBy [ backgroundColor.ColorId ].Select( bg => bg.Month );
        backgroundColor.Months = months.ToArray();
        backgroundColor.DisplayMonths = months.ToFormatString();
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

    #region Save number of months
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var project = await _projectService.GetProject( userId, projectId );
    if ( project == null ) {
      return BadRequest( ProjectSettingNotification.NonExisted );
    }
    if ( project.NumberOfMonths > formData.NumberOfMonths ) {
      await _backgroundService.BatchDelete( projectId, formData.NumberOfMonths + 1 );
    }
    else if ( project.NumberOfMonths < formData.NumberOfMonths ) {
      await _backgroundService.AddMonth( projectId, formData.NumberOfMonths - project.NumberOfMonths );
    }
    #endregion

    #region 
    var setting = await _projectSettingService.GetProjectSetting( projectId );
    if ( setting is null )
      return BadRequest( ProjectSettingNotification.NonExisted );

    setting.SeparateGroupTask = formData.SeparateGroupTask;
    setting.AssemblyDurationRatio = formData.AssemblyDurationRatio;
    setting.RemovalDurationRatio = formData.RemovalDurationRatio;
    setting.ColumnWidth = formData.ColumnWidth;
    setting.AmplifiedFactor = formData.AmplifiedFactor;

    var result = await _projectSettingService.UpdateProjectSetting( setting );
    if ( !result.Success )
      return BadRequest( result.Message );
    #endregion

    #region Save stepwork color
    var stepworkColors = await _colorDefService.GetStepworkColorDefsByProjectId( projectId );
    foreach ( var stepworkColorData in formData.StepworkColors ) {
      if ( stepworkColorData.ColorId == null ) {
        var newColor = new ColorDef()
        {
          Name = stepworkColorData.Name,
          Code = stepworkColorData.Code,
          Type = 2,
          ProjectId = projectId
        };
        await _colorDefService.CreateColorDef( newColor );
      }
      else {
        var existingColor = stepworkColors.FirstOrDefault( color => color.ColorId == stepworkColorData.ColorId );
        if ( existingColor == null ) {
          continue;
        }
        if ( !existingColor.IsDefault )
          existingColor.Name = stepworkColorData.Name;
        existingColor.Code = stepworkColorData.Code;
        await _colorDefService.UpdateColorDef( existingColor );
      }
    }
    var stepworkColorData1 = formData.StepworkColors.Where( color => color.ColorId != null );
    var deletedStepworkColors = stepworkColors.Where( color => !stepworkColorData1.Any( x => x.ColorId == color.ColorId ) );
    foreach ( var color in deletedStepworkColors ) {
      await _colorDefService.DeleteColorDef( color.ColorId );
    }
    #endregion

    #region Save background color
    var backgroundColors = await _colorDefService.GetBackgroundColorDefsByProjectId( projectId );
    foreach ( var backgroundColorData in formData.BackgroundColors ) {
      if ( backgroundColorData.ColorId == null ) {
        var newColor = new ColorDef()
        {
          Name = backgroundColorData.Name,
          Code = backgroundColorData.Code,
          Type = 1,
          ProjectId = projectId
        };
        var bgResult = await _colorDefService.CreateColorDef( newColor );
        if ( bgResult.Success ) {
          backgroundColorData.ColorId = bgResult.Content.ColorId;
        }
      }
      else {
        var existingColor = backgroundColors.FirstOrDefault( color => color.ColorId == backgroundColorData.ColorId );
        if ( existingColor == null ) {
          continue;
        }
        existingColor.Name = backgroundColorData.Name;
        existingColor.Code = backgroundColorData.Code;
        await _colorDefService.UpdateColorDef( existingColor );
      }
    }

    var backgrounds = await _backgroundService.GetBackgroundsByProjectId( projectId );
    foreach ( var bg in backgrounds ) {
      var bgData = formData.BackgroundColors.FirstOrDefault( color => color.DisplayMonths.ToNumberArray().Any( x => x == bg.Month ) );
      if ( bgData == null ) {
        bg.ColorId = null;
      }
      else {
        bg.ColorId = bgData.ColorId;
      }

      await _backgroundService.UpdateProjectBackground( bg );
    }

    var bgColorData = formData.BackgroundColors.Where( color => color.ColorId != null );
    var deletedBgColors = backgroundColors.Where( color => !bgColorData.Any( x => x.ColorId == color.ColorId ) );
    foreach ( var color in deletedBgColors ) {
      await _colorDefService.DeleteColorDef( color.ColorId );
    }
    #endregion

    return NoContent();
  }
}
