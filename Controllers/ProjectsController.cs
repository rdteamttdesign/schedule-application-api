using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using SchedulingTool.Api.Resources.projectdetail;
using System.Drawing.Printing;
using System.Xml.Linq;

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

  [HttpGet( "name-list" )]
  [Authorize]
  public async Task<IActionResult> GetProjectNameList()
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var projects = await _projectService.GetActiveProjects( userId );
    var nameList = projects.Select( p => p.ProjectName ).ToList();

    return Ok( nameList );
  }

  [HttpGet()]
  [Authorize]
  public async Task<IActionResult> GetProjects( [FromQuery] QueryProjectFormData formData )
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var projects = await _projectService.GetActiveProjects( userId );
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

    var pagedListprojects = PagedList<Project>.ToPagedList( projects.OrderByDescending( project => project.ModifiedDate ), formData.PageNumber, formData.PageSize );

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
      ModifiedDate = DateTime.Now,
      IsActivated = true,
      NumberOfMonths = formData.NumberOfMonths
    };
    var result = await _projectService.CreateProject( newProject );

    if ( !result.Success ) {
      return BadRequest( result.Message );
    }

    var resource = _mapper.Map<ProjectResource>( result.Content );

    return Ok( new { Project = resource, Details = SampleProjectDetail() } );
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
    //existingProject.NumberOfMonths = formData.NumberOfMonths;

    var result = await _projectService.UpdateProject( existingProject );
    if ( !result.Success )
      return BadRequest( result.Message );

    var resource = _mapper.Map<ProjectResource>( result.Content );
    return Ok( resource );
  }

  private ICollection<object> SampleProjectDetail()
  {
    var groupTaskId1 = Guid.NewGuid().ToString();
    var taskId11 = Guid.NewGuid().ToString();
    var taskId12 = Guid.NewGuid().ToString();
    var groupTaskId2 = Guid.NewGuid().ToString();
    var taskId21 = Guid.NewGuid().ToString();
    return new object []
    {
      new GroupTaskResource()
      {
        Start = 0,
        Duration = 30,
        Name = "Group 1",
        Id = groupTaskId1,
        Type = "project",
        HideChildren = false,
        DisplayOrder = 1,
        GroupsNumber = 1
      },
      new TaskResource()
      {
        Start = 0,
        Duration = 30,
        Name = "Task 1",
        Id = taskId11,
        Type = "task",
        Detail= "",
        GroupId = groupTaskId1,
        DisplayOrder = 2,
        Note = "",
        //ColorId= "",
        GroupsNumber = 1,
        Stepworks = new StepworkResource[]
        {
          new StepworkResource()
          {
            Start = 0,
            Duration = 30,
            PercentStepWork = 50,
            Name = "",
            ParentTaskId = taskId11,
            Id = Guid.NewGuid().ToString(),
            Type = "task",
            GroupId = groupTaskId1,
            DisplayOrder = 2,
            Predecessors = new PredecessorResource[] { },
            //ColorId= ""
          },
          new StepworkResource()
          {
            Start = 70,
            Duration = 30,
            PercentStepWork = 50,
            Name = "",
            ParentTaskId = taskId11,
            Id = Guid.NewGuid().ToString(),
            Type = "task",
            GroupId = groupTaskId1,
            DisplayOrder = 2,
            Predecessors = new PredecessorResource[] { },
            //ColorId= ""
          }
        }
      },
      new TaskResource()
      {
        Start= 140,
        Duration= 30,
        Name = "Task 2",
        Id = taskId12,
        Predecessors = new PredecessorResource [] { },
        Type = "task",
        GroupId = groupTaskId1,
        DisplayOrder = 3,
        Note = "",
        GroupsNumber = 1,
        ColorId = 1,
      },
      new GroupTaskResource()
      {
        Start = 0,
        Duration = 30,
        Name = "Group 2",
        Id = groupTaskId2,
        Type = "project",
        HideChildren = false,
        DisplayOrder = 4,
        GroupsNumber = 1
      },
      new TaskResource()
      {
        Start= 140,
        Duration= 30,
        Name = "Task 2",
        Id = taskId21,
        Predecessors = new PredecessorResource [] { },
        Type = "task",
        GroupId = groupTaskId2,
        DisplayOrder = 5,
        Note = "",
        GroupsNumber = 1,
        ColorId = 1,
      },
    };
  }
}
