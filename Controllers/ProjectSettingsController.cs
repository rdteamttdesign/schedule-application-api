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
  private readonly IVersionService _versionService;

  public ProjectSettingsController(
    IMapper mapper,
    IProjectSettingService projectSettingService,
    IColorDefService colorDefService,
    IBackgroundService backgroundService,
    IVersionService versionService )
  {
    _mapper = mapper;
    _projectSettingService = projectSettingService;
    _colorDefService = colorDefService;
    _backgroundService = backgroundService;
    _versionService = versionService;
  }

  [HttpGet( "versions/{versionId}/settings" )]
  [Authorize]
  public async Task<IActionResult> GetVersionSetting( long versionId )
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var version = await _versionService.GetVersionById( versionId );
    if ( version == null ) {
      return BadRequest( ProjectSettingNotification.NonExisted );
    }
    var setting = await _projectSettingService.GetProjectSetting( versionId );
    var resource = _mapper.Map<ProjectSettingResource>( setting );
    resource.NumberOfMonths = version.NumberOfMonths;

    var stepworkColors = await _colorDefService.GetStepworkColorDefsByVersionId( versionId );
    resource.StepworkColors = _mapper.Map<IEnumerable<ColorDefResource>>( stepworkColors ).ToList();

    var backgroundColors = await _colorDefService.GetBackgroundColorDefsByVersionId( versionId );
    resource.BackgroundColors = _mapper.Map<IEnumerable<BackgroundColorResource>>( backgroundColors ).ToList();

    var backgrounds = await _backgroundService.GetBackgroundsByVersionId( versionId );
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

    #region Save number of months
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var version = await _versionService.GetVersionById(versionId);
    if ( version == null ) {
      return BadRequest( ProjectSettingNotification.NonExisted );
    }
    if ( version.NumberOfMonths > formData.NumberOfMonths ) {
      await _backgroundService.BatchDelete( versionId, formData.NumberOfMonths + 1 );
    }
    else if ( version.NumberOfMonths < formData.NumberOfMonths ) {
      await _backgroundService.AddMonth( versionId, formData.NumberOfMonths - version.NumberOfMonths );
    }
    #endregion

    #region 
    var setting = await _projectSettingService.GetProjectSetting( versionId );
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
    var stepworkColors = await _colorDefService.GetStepworkColorDefsByVersionId( versionId );
    foreach ( var stepworkColorData in formData.StepworkColors ) {
      if ( stepworkColorData.ColorId == null ) {
        var newColor = new ColorDef()
        {
          Name = stepworkColorData.Name,
          Code = stepworkColorData.Code,
          Type = 2,
          VersionId = versionId
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
    var backgroundColors = await _colorDefService.GetBackgroundColorDefsByVersionId( versionId );
    foreach ( var backgroundColorData in formData.BackgroundColors ) {
      if ( backgroundColorData.ColorId == null ) {
        var newColor = new ColorDef()
        {
          Name = backgroundColorData.Name,
          Code = backgroundColorData.Code,
          Type = 1,
          VersionId = versionId
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

    var backgrounds = await _backgroundService.GetBackgroundsByVersionId( versionId );
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
