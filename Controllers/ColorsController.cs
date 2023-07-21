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
public class ColorsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IColorDefService _colorDefService;

  public ColorsController( IMapper mapper, IColorDefService colorDefService )
  {
    _mapper = mapper;
    _colorDefService = colorDefService;
  }

  [HttpGet( "project/{projectId}/background-colors" )]
  [Authorize]
  public async Task<IActionResult> GetBackgroundColorDefs( long projectId )
  {
    var colors = await _colorDefService.GetBackgroundColorDefsByVersionId( projectId );
    if ( !colors.Any() ) {
      return BadRequest( ColorDefNotification.NonExisted );
    }
    var resource = _mapper.Map<IEnumerable<ColorDefResource>>( colors );
    return Ok( resource );
  }

  [HttpGet( "project/{projectId}/stepwork-colors" )]
  [Authorize]
  public async Task<IActionResult> GetStepworkColorDefs( long projectId )
  {
    var colors = await _colorDefService.GetStepworkColorDefsByVersionId( projectId );
    if ( !colors.Any() ) {
      return BadRequest( ColorDefNotification.NonExisted );
    }
    var resource = _mapper.Map<IEnumerable<ColorDefResource>>( colors );
    return Ok( resource );
  }

  [HttpPost( "project/{projectId}/colors" )]
  [Authorize]
  public async Task<IActionResult> CreateColor( long projectId, [FromBody] ColorFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var newColor = new ColorDef()
    {
      Name = formData.Name,
      Code = formData.Code,
      Type = formData.Type,
      VersionId = projectId
    };
    var result = await _colorDefService.CreateColorDef( newColor );
    if ( !result.Success ) {
      return BadRequest( result.Message );
    }
    var resource = _mapper.Map<ColorDefResource>( result.Content );
    return Ok( resource );
  }

  [HttpPut( "colors/{colorId}" )]
  [Authorize]
  public async Task<IActionResult> UpdateColor( long colorId, [FromBody] ColorFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var existingColor = await _colorDefService.GetColor( colorId );
    if ( existingColor is null )
      return BadRequest( ColorDefNotification.NonExisted );

    if ( !existingColor.IsDefault )
      existingColor.Name = formData.Name;
    existingColor.Code = formData.Code;

    var result = await _colorDefService.UpdateColorDef( existingColor );
    if ( !result.Success )
      return BadRequest( result.Message );

    var resource = _mapper.Map<ColorDefResource>( result.Content );
    return Ok( resource );
  }

  [HttpDelete( "colors/{colorId}" )]
  [Authorize]
  public async Task<IActionResult> DeleteColor( long colorId )
  {
    var result = await _colorDefService.DeleteColorDef( colorId );

    if ( !result.Success ) {
      return BadRequest( result.Message );
    }

    var resource = _mapper.Map<ColorDefResource>( result.Content );
    return Ok( resource );
  }
}
