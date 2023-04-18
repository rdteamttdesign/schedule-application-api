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

namespace SchedulingTool.Api.Controllers;

[Route( "api/[controller]" )]
[ApiController]
public class ProjectsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IProjectService _projectService;
  private readonly IProjectSettingService _projectSetting;
  private readonly IGroupTaskService _groupTaskService;
  private readonly ITaskService _taskService;
  private readonly IStepworkService _stepworkService;
  private readonly IPredecessorService _predecessorService;
  private readonly IColorDefService _colorService;

  public ProjectsController( 
    IMapper mapper, 
    IProjectService projectService,
    IProjectSettingService projectSetting,
    IGroupTaskService groupTaskService,
    ITaskService taskService,
    IStepworkService stepworkService,
    IPredecessorService predecessorService,
    IColorDefService colorService )
  {
    _mapper = mapper;
    _projectService = projectService;
    _groupTaskService = groupTaskService;
    _taskService = taskService;
    _stepworkService = stepworkService;
    _predecessorService = predecessorService;
    _colorService = colorService;
    _projectSetting = projectSetting;
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

    var colors = await _colorService.GetStepworkColorDefsByProjectId( newProject.ProjectId );
    var installColor = colors.FirstOrDefault( c => c.IsInstall == 0 );
    var removalColor = colors.FirstOrDefault( c => c.IsInstall == 1 );

    var setting = await _projectSetting.GetProjectSetting( result.Content.ProjectId );
    await SaveProjectTasks( result.Content.ProjectId, SampleProjectDetail( setting, installColor!.ColorId, removalColor!.ColorId ) );

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
    //existingProject.NumberOfMonths = formData.NumberOfMonths;

    var result = await _projectService.UpdateProject( existingProject );
    if ( !result.Success )
      return BadRequest( result.Message );

    var resource = _mapper.Map<ProjectResource>( result.Content );
    return Ok( resource );
  }

  private ICollection<GroupTaskFormData> SampleProjectDetail( ProjectSetting setting, long installColorId, long removalColorId )
  {
    var groupTaskId1 = Guid.NewGuid().ToString();
    var taskId11 = Guid.NewGuid().ToString();
    var taskId12 = Guid.NewGuid().ToString();
    var groupTaskId2 = Guid.NewGuid().ToString();
    var taskId21 = Guid.NewGuid().ToString();
    var columnWidth = 70;
    return new GroupTaskFormData []
    {
      new GroupTaskFormData()
      {
        Start = 0,
        Duration = 30,
        Name = "Group 1",
        Id = groupTaskId1,
        Type = "project",
        HideChildren = false,
        DisplayOrder = 1,
        GroupsNumber = 1,
        ColorId = installColorId
      },
      new GroupTaskFormData()
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
        ColorId= installColorId,
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
            Predecessors = Array.Empty<PredecessorResource>(),
            ColorId= installColorId,
            End = 15 * columnWidth / 30*setting.AmplifiedFactor
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
            Predecessors = Array.Empty<PredecessorResource>(),
            ColorId= removalColorId,
            End = 70 + 15 * columnWidth / 30 * setting.AmplifiedFactor
          }
        }
      },
      new GroupTaskFormData()
      {
        Start= 140,
        Duration = 30,
        Name = "Task 2",
        Id = taskId12,
        Predecessors = Array.Empty < PredecessorResource >(),
        Type = "task",
        GroupId = groupTaskId1,
        DisplayOrder = 3,
        Note = "",
        GroupsNumber = 1,
        ColorId = installColorId,
        End = 140 + 30 * columnWidth / 30*setting.AmplifiedFactor
      },
      new GroupTaskFormData()
      {
        Start = 0,
        Duration = 30,
        Name = "Group 2",
        Id = groupTaskId2,
        Type = "project",
        HideChildren = false,
        DisplayOrder = 4,
        GroupsNumber = 1,
        ColorId = installColorId,
        End = 30 * columnWidth / 30 * setting.AmplifiedFactor
      },
      new GroupTaskFormData()
      {
        Start= 140,
        Duration= 30,
        Name = "Task 3",
        Id = taskId21,
        Predecessors = Array.Empty<PredecessorResource>(),
        Type = "task",
        GroupId = groupTaskId2,
        DisplayOrder = 5,
        Note = "",
        GroupsNumber = 1,
        ColorId = installColorId,
        End = 140 + 30 * columnWidth / 30 * setting.AmplifiedFactor
      },
    };
  }

  [HttpGet( "{projectId}/details" )]
  [Authorize]
  public async Task<IActionResult> GetProjectDetails( long projectId )
  {
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var project = await _projectService.GetProject( userId, projectId );
    if ( project == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    try {
      var groupTaskResources = await GetGroupTasksByProjectId( projectId );
      return Ok( groupTaskResources );
    }
    catch ( Exception ex ) {
      return BadRequest( $"Something went wrong: {ex.Message}" );
    }
  }

  private async Task<IEnumerable<object>> GetGroupTasksByProjectId( long projectId )
  {
    var setting = await _projectSetting.GetProjectSetting( projectId );
    var result = new List<KeyValuePair<int, object>>();
    var groupTasks = await _groupTaskService.GetGroupTasksByProjectId( projectId );
    var groupTaskResources = _mapper.Map<List<GroupTaskResource>>( groupTasks );
    groupTaskResources.ForEach( g => result.Add( new KeyValuePair<int, object>( g.DisplayOrder, g ) ) );

    var stepworkColors = ( await _colorService.GetStepworkColorDefsByProjectId( projectId ) ).ToDictionary( x => x.ColorId, x => x.Code );

    foreach ( var groupTask in groupTasks ) {
      var tasks = await _taskService.GetTasksByGroupTaskId( groupTask.GroupTaskId );
      foreach ( var task in tasks ) {

        var stepworks = await _stepworkService.GetStepworksByTaskId( task.TaskId );
        var taskResource = _mapper.Map<TaskResource>( task );
        taskResource.Duration = task.Duration;

        if ( stepworks.Count() == 1 ) {
          var predecessors = await _predecessorService.GetPredecessorsByStepworkId( stepworks.First().StepworkId );
          var predecessorResources = _mapper.Map<List<PredecessorResource>>( predecessors );
          taskResource.Predecessors = predecessorResources.Count == 0 ? null : predecessorResources;
          taskResource.ColorId = stepworks.First().ColorId;
          taskResource.Start = stepworks.First().Start;
          taskResource.End = stepworks.First().End;
        }
        else {
          var stepworkResources = new List<StepworkResource>();
          foreach ( var stepwork in stepworks ) {
            if ( stepwork.Portion == 1 ) {
              continue;
            }
            var predecessors = await _predecessorService.GetPredecessorsByStepworkId( stepwork.StepworkId );
            var predecessorResources = _mapper.Map<List<PredecessorResource>>( predecessors );
            var stepworkResource = _mapper.Map<StepworkResource>( stepwork );
            stepworkResource.PercentStepWork *= 100;
            stepworkResource.Start = stepwork.Start;
            stepworkResource.Duration = task.Duration;
            stepworkResource.End = stepwork.End;
            stepworkResource.GroupId = groupTask.LocalId;
            stepworkResource.Predecessors = predecessorResources.Count == 0 ? null : predecessorResources;
            stepworkResources.Add( stepworkResource );
          }
          taskResource.Stepworks = stepworkResources.Count == 0 ? null : stepworkResources;
        }
        result.Add( new KeyValuePair<int, object>( taskResource.DisplayOrder, taskResource ) );
      }
    }
    return result.OrderBy( o => o.Key ).Select( o => o.Value );
  }

  [HttpPost( "{projectId}/details" )]
  [Authorize]
  public async Task<IActionResult> SaveProjectDetails( long projectId, [FromBody] ICollection<GroupTaskFormData> formData )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var userId = long.Parse( HttpContext.User.Claims.FirstOrDefault( x => x.Type.ToLower() == "sid" )?.Value! );
    var project = await _projectService.GetProject( userId, projectId );
    if ( project == null ) {
      return BadRequest( ProjectNotification.NonExisted );
    }
    try {
      await _projectService.BatchDeleteProjectDetails( projectId );
    }
    catch ( Exception ex ) {
      return BadRequest( ex.Message );
    }

    await SaveProjectTasks( projectId, formData );

    return NoContent();
  }

  private async Task SaveProjectTasks( long projectId, ICollection<GroupTaskFormData> formData )
  {
    var converter = new ModelConverter( projectId, formData );

    var grouptasks = new Dictionary<string, GroupTask>();
    foreach ( var grouptask in converter.GroupTasks ) {
      var result = await _groupTaskService.CreateGroupTask( grouptask );
      if ( result.Success ) {
        grouptasks.Add( result.Content.LocalId, result.Content );
      }
    }

    var tasks = new Dictionary<string, ModelTask>();
    foreach ( var task in converter.Tasks ) {
      task.GroupTaskId = grouptasks [ task.GroupTaskLocalId ].GroupTaskId;
      var result = await _taskService.CreateTask( task );
      if ( result.Success ) {
        tasks.Add( result.Content.LocalId, result.Content );
      }
    }

    var stepworks = new Dictionary<string, Stepwork>();
    foreach ( var stepwork in converter.Stepworks ) {
      stepwork.TaskId = tasks [ stepwork.TaskLocalId ].TaskId;
      var result = await _stepworkService.CreateStepwork( stepwork );
      if ( result.Success ) {
        stepworks.Add( result.Content.LocalId, result.Content );
      }
    }

    foreach ( var predecessor in converter.Predecessors ) {
      if ( !stepworks.ContainsKey( predecessor.StepworkId ) || !stepworks.ContainsKey( predecessor.RelatedStepworkId ) ) {
        continue;
      }
      await _predecessorService.CreatePredecessor( new Predecessor()
      {
        StepworkId = stepworks [ predecessor.StepworkId ].StepworkId,
        RelatedStepworkId = stepworks [ predecessor.RelatedStepworkId ].StepworkId,
        RelatedStepworkLocalId = predecessor.RelatedStepworkId,
        Type = predecessor.Type,
        Lag = predecessor.Lag
      } );
    }
  }

  [HttpPost( "{projectId}/import" ), DisableRequestSizeLimit]
  [Authorize]
  public async Task<IActionResult> Import( long projectId )
  {
    if ( !ModelState.IsValid ) {
      return BadRequest( ModelState.GetErrorMessages() );
    }
    var colors = await _colorService.GetStepworkColorDefsByProjectId( projectId );
    var installColor = colors.FirstOrDefault( c => c.IsInstall == 0 );
    var removalColor = colors.FirstOrDefault( c => c.IsInstall == 1 );

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

    var sheetNameList = formCollection [ "SheetName" ];
    var setting = await _projectSetting.GetProjectSetting( projectId );
    var result = ImportFileUtils.ReadFromFile( file.OpenReadStream(), sheetNameList, setting!, installColor!.ColorId, removalColor!.ColorId, maxDisplayOrder );
    return Ok( result );
  }

  //[HttpGet( "{projectId}/download" )]
  //[Authorize]
  //public async Task<IActionResult> DownloadFile()
  //{
  //  return Ok();
  //}
}
