using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using System.Drawing.Printing;

namespace SchedulingTool.Api.Controllers;

[Route( "api/[controller]" )]
[ApiController]
public class ProjectsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IProjectService _projectService;

  public ProjectsController( IMapper mapper, IProjectService projectService )
  {
    _mapper = mapper;
    _projectService = projectService;
  }

  [HttpGet( "{projectId}" )]
  [Authorize]
  public async Task<IActionResult> GetProjectById( long projectId )
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var project = await _projectService.GetProject( userId, projectId );
    if ( project == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    var resource = _mapper.Map<ProjectResource>( project );
    return Ok( resource );
  }

  [HttpGet()]
  [Authorize]
  public async Task<IActionResult> GetProjects( [FromBody] QueryProjectFormData formData)
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var projects = await _projectService.GetActiveProjects( userId );
    if ( !projects.Any() ) {
      return BadRequest( ProjectNotification.NonExisted );
    }

    var pagedListprojects = PagedList<Project>.ToPagedList( projects, formData.PageNumber, formData.PageSize );
    
    var resources = _mapper.Map<IEnumerable<ProjectResource>>( pagedListprojects );
    return Ok( new
    {
      Data = resources,
      CurrentPage = pagedListprojects.CurrentPage,
      PageSize = pagedListprojects.PageSize,
      PageCount = pagedListprojects.TotalPages,
      HasNext = pagedListprojects.HasNext,
      HasPrevious = pagedListprojects.HasPrevious
    } );
  }

  [HttpPost()]
  [Authorize]
  public async Task<IActionResult> CreateProject( [FromBody] ProjectFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var newProject = new Project()
    {

      ProjectName = formData.ProjectName,
      UserId = userId,
      CreatedDate = DateTime.Now,
      IsActivated = true,
      NumberOfMonths = formData.NumberOfMonths
    };
    var result = await _projectService.CreateProject( newProject );

    if ( !result.Success ) {
      return BadRequest( result.Message );
    }

    var resource = _mapper.Map<ProjectResource>( result.Content );

    return Ok( resource );
  }

  [HttpPut( "deactive-projects" )]
  [Authorize]
  public async Task<IActionResult> DeactiveProjects( [FromBody] ICollection<long> projectIds )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    try {
      await _projectService.BatchDeactiveProjects( userId, projectIds );
      return NoContent();
    }
    catch ( Exception ex ) {
      return BadRequest( $"{ProjectNotification.ErrorSaving} {ex.Message}" );
    }
  }

  [HttpPut( "{projectId}" )]
  [Authorize]
  public async Task<IActionResult> UpdateProject( long projectId, [FromBody] ProjectFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var existingProject = await _projectService.GetProject( userId, projectId );
    if ( existingProject is null )
      return BadRequest( ProjectNotification.NonExisted );

    existingProject.ProjectName = formData.ProjectName;
    existingProject.NumberOfMonths = formData.NumberOfMonths;

    var result = await _projectService.UpdateProject( existingProject );
    if ( !result.Success )
      return BadRequest( result.Message );

    var resource = _mapper.Map<ProjectResource>( result.Content );
    return Ok( resource );
  }
}
