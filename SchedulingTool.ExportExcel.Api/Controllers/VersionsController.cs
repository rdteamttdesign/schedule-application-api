using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Services;

namespace SchedulingTool.Api.Controllers;

[Route( "api/[controller]" )]
[ApiController]
public class VersionsController : ControllerBase
{
  private readonly IVersionService _versionService;

  public VersionsController( IVersionService versionService )
  {
    _versionService = versionService;
  }

  [HttpGet( "{versionId}/download" )]
  public async Task<IActionResult> DownloadFile( long versionId )
  {
    var result = await _versionService.ExportToExcel( versionId );
    if ( result.Success ) {
      var fileBytes = System.IO.File.ReadAllBytes( result.Content.FilePath );
      return File( fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, $"{Guid.NewGuid()}.xlsx" );
    }
    else {
      return BadRequest( result.Message );
    }
  }
}