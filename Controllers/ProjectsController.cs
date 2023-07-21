using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using SchedulingTool.Api.Resources.FormBody.projectdetail;
using SchedulingTool.Api.Resources.projectdetail;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;
using Task = System.Threading.Tasks.Task;
using Version = SchedulingTool.Api.Domain.Models.Version;

namespace SchedulingTool.Api.Controllers;

[Route( "api/[controller]" )]
[ApiController]
public class ProjectsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IProjectService _projectService;

  public ProjectsController(
    IMapper mapper,
    IProjectService projectService )
  {
    _mapper = mapper;
    _projectService = projectService;
  } 

  [HttpGet()]
  //[Authorize]
  public async Task<IActionResult> GetProjects( [FromQuery] QueryProjectFormData formData )
  {
    var userId = 1;//long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var projects = await _projectService.GetProjectListByUserId( userId );
    if ( !projects.Any() ) {
      return Ok( new
      {
        Data = new object [] { },
        CurrentPage = 0,
        PageSize = 0,
        PageCount = 0,
        HasNext = false,
        HasPrevious = false
      } );
    }

    var pagedListprojects = PagedList<ProjectListResource>.ToPagedList( projects.OrderByDescending( project => project.ModifiedDate ), formData.PageNumber, formData.PageSize );

    //var resources = _mapper.Map<IEnumerable<ProjectListResource>>( pagedListprojects );
    return Ok( new
    {
      Data = pagedListprojects,
      CurrentPage = pagedListprojects.CurrentPage,
      PageSize = pagedListprojects.PageSize,
      PageCount = pagedListprojects.TotalPages,
      HasNext = pagedListprojects.HasNext,
      HasPrevious = pagedListprojects.HasPrevious
    } );
  }

  [HttpPost()]
  //[Authorize]
  public async Task<IActionResult> CreateProject( [FromBody] NewProjectFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = 1;//long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var version = new Version()
    {
      VersionName = formData.VersionName,
      UserId = userId,
      CreatedDate = DateTime.Now,
      ModifiedDate = DateTime.Now,
      IsActivated = true,
      NumberOfMonths = formData.NumberOfMonths
    };
    var project = new Project()
    {
      ProjectName = formData.ProjectName,
      UserId = userId,
      CreatedDate = DateTime.Now,
      ModifiedDate = DateTime.Now
    };
    var result = await _projectService.CreateProject( project, version );

    if ( !result.Success ) {
      return BadRequest( result.Message );
    }

    return Ok( result.Content );
  }

  [HttpPut( "{projectId}" )]
  //[Authorize]
  public async Task<IActionResult> UpdateProject( long projectId, [FromBody] UpdateProjectFormData formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    //var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var existingProject = await _projectService.GetProjectById( projectId );
    if ( existingProject is null )
      return BadRequest( ProjectNotification.NonExisted );

    existingProject.ProjectName = formData.ProjectName;
    existingProject.ModifiedDate = DateTime.Now;

    var result = await _projectService.UpdateProject( existingProject );
    if ( !result.Success )
      return BadRequest( result.Message );

    var resource = _mapper.Map<ProjectResource>( result.Content );
    return Ok( resource );
  }
}
