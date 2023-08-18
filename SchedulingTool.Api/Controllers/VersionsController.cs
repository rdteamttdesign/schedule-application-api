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
public class VersionsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IVersionService _versionService;
  private readonly IViewService _viewService;
  private readonly IViewTaskService _viewTaskService;

  public VersionsController(
    IMapper mapper,
    IVersionService versionService,
    IViewService viewService,
    IViewTaskService viewTaskService )
  {
    _mapper = mapper;
    _versionService = versionService;
    _viewService = viewService;
    _viewTaskService = viewTaskService;
  }

  [HttpPut( "versions/deactive-versions" )]
  [Authorize]
  public async Task<IActionResult> DeactiveVersions( [FromBody] ICollection<long> versionIds )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    try {
      await _versionService.BatchDeactiveVersions( userId, versionIds );
      return NoContent();
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ProjectNotification.ErrorSaving} {ex.Message}. {ex.InnerException?.Message}" );
    }
  }

  [HttpPut( "versions/{versionId}" )]
  [Authorize]
  public async Task<IActionResult> UpdateVersion( long versionId, [FromBody] UpdateVersionFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var existingVersion = await _versionService.GetVersionById( versionId );
    if ( existingVersion is null )
      return BadRequest( ProjectNotification.NonExisted );

    existingVersion.VersionName = formData.VersionName;
    existingVersion.ModifiedDate = DateTime.UtcNow;

    var result = await _versionService.UpdateVersion( existingVersion );
    if ( !result.Success )
      return BadRequest( result.Message );

    var resource = _mapper.Map<VersionResource>( result.Content );

    return Ok( resource );
  }

  [HttpGet( "versions/{versionId}/details" )]
  [Authorize]
  public async Task<IActionResult> GetProjectDetails( long versionId, int columnWidth, double amplifiedFactor )
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var version = await _versionService.GetVersionById( versionId );
    if ( version == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    try {
      var groupTaskResources = await _versionService.GetGroupTasksByVersionId( versionId, columnWidth, amplifiedFactor );
      return Ok( groupTaskResources );
    }
    catch ( Exception ex ) {
      return BadRequest( $"Something went wrong: {ex.Message}. {ex.InnerException?.Message}" );
    }
  }

  [HttpPost( "versions/{versionId}/details" )]
  [Authorize]
  public async Task<IActionResult> SaveProjectDetails( long versionId, [FromBody] ICollection<CommonGroupTaskFormData> formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var version = await _versionService.GetVersionById( versionId );
    if ( version == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    try {
      await _versionService.BatchDeleteVersionDetails( versionId );
    }
    catch ( Exception ex ) {
      return BadRequest( $"Something went wrong: {ex.Message}. {ex.InnerException?.Message}" );
    }

    await _versionService.SaveProjectTasks( versionId, formData );

    version.ModifiedDate = DateTime.UtcNow;
    await _versionService.UpdateVersion( version );

    return NoContent();
  }

  [HttpPost( "versions/{versionId}/import" ), DisableRequestSizeLimit]
  [Authorize]
  public async Task<IActionResult> Import( long versionId )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }

    var formCollection = await Request.ReadFormAsync();
    var file = formCollection.Files.First();

    if ( file.Length <= 0 ) {
      return BadRequest( "No file found." );
    }

    if ( !formCollection.ContainsKey( "SheetName" ) ) {
      return BadRequest( "No sheet name found." );
    }

    if ( !formCollection.ContainsKey( "DisplayOrder" ) ) {
      return BadRequest( "Display Order field missing." );
    }

    if ( !int.TryParse( formCollection [ "DisplayOrder" ].ToString(), out var maxDisplayOrder ) ) {
      return BadRequest( "Display Order is not a number." );
    }

    var sheetNameList = formCollection [ "SheetName" ].ToString().Split( "," );

    var result = await _versionService.GetDataFromFile( versionId, file.OpenReadStream(), maxDisplayOrder, sheetNameList );

    return Ok( result );
  }

  [HttpPost( "projects/{projectId}/versions/{versionId}/duplicate" )]
  [Authorize]
  public async Task<IActionResult> DuplicateVersion( long projectId, long versionId )
  {
    try {
      var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
      var version = await _versionService.GetVersionById( versionId );
      if ( version == null ) {
        return BadRequest( ProjectNotification.NonExisted );
      }
      var result = await _versionService.DuplicateVersion( projectId, version );
      await _viewService.DuplicateView( versionId, result.Content.VersionId );
      return Ok( result.Content );
    }
    catch ( Exception ex ) {
      return BadRequest( $"Something went wrong: {ex.Message}. {ex.InnerException?.Message}" );
    }
  }

  [HttpDelete( "versions/delete-versions" )]
  [Authorize]
  public async Task<IActionResult> DeleteVersions( [FromBody] ICollection<long> versionIds )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    try {
      await _versionService.BatchDeleteVersions( versionIds );
      return NoContent();
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ProjectNotification.ErrorSaving} {ex.Message}. {ex.InnerException?.Message}" );
    }
  }

  [HttpPut( "versions/activate-versions" )]
  [Authorize]
  public async Task<IActionResult> ActivateVersions( [FromBody] ICollection<long> versionIds )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    try {
      await _versionService.BatchActivateVersions( userId, versionIds );
      return NoContent();
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ProjectNotification.ErrorSaving} {ex.Message}. {ex.InnerException?.Message}" );
    }
  }

  [HttpPut( "versions/{versionId}/update-from-excel" ), DisableRequestSizeLimit]
  [Authorize]
  public async Task<IActionResult> UpdateFromExcelFile( long versionId )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }

    try {
      var formCollection = await Request.ReadFormAsync();
      var file = formCollection.Files.First();

      if ( file.Length <= 0 ) {
        return BadRequest( "No file found." );
      }

      if ( !formCollection.ContainsKey( "SheetName" ) ) {
        return BadRequest( "No sheet name found." );
      }

      var sheetNameList = formCollection [ "SheetName" ].ToString().Split( "," );
      var result = await _versionService.GetUpdatedDataFromFile( versionId, file.OpenReadStream(), sheetNameList );

      return Ok( result );

    }
    catch ( Exception ex ) {
      return BadRequest( ex.Message );
    }
  }

  [HttpPut( "versions/send-to-another-project" )]
  [Authorize]
  public async Task<IActionResult> SendVersionsToAnotherProject( [FromBody] SendVersionsToAnotherProjectFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    try {
      await _versionService.SendVersionsToAnotherProject( formData.VersionIds, formData.ToProject );
      return NoContent();
    }
    catch ( Exception ex ) {
      return BadRequest( ex.Message );
    }
  }
}
