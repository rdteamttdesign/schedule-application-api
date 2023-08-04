using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Models.Enum;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Resources;
using System.Collections;

namespace SchedulingTool.Api.Controllers;

[Route( "api/[controller]" )]
[ApiController]
public class VersionsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IVersionService _versionService;
  private readonly IProjectSettingService _projectSettingService;
  private readonly IGroupTaskService _groupTaskService;
  private readonly ITaskService _taskService;
  private readonly IStepworkService _stepworkService;
  private readonly IPredecessorService _predecessorService;
  private readonly IColorDefService _colorService;
  private readonly IBackgroundService _backgroundService;
  private readonly IViewService _viewService;

  public VersionsController(
    IMapper mapper,
    IVersionService versionService,
    IProjectSettingService projectSettingService,
    IGroupTaskService groupTaskService,
    ITaskService taskService,
    IStepworkService stepworkService,
    IPredecessorService predecessorService,
    IColorDefService colorService,
    IBackgroundService backgroundService,
    IViewService viewService )
  {
    _mapper = mapper;
    _groupTaskService = groupTaskService;
    _taskService = taskService;
    _stepworkService = stepworkService;
    _predecessorService = predecessorService;
    _colorService = colorService;
    _projectSettingService = projectSettingService;
    _versionService = versionService;
    _backgroundService = backgroundService;
    _viewService = viewService;
  }

  [HttpGet( "{versionId}/download" )]
  public async Task<IActionResult> DownloadFile( long versionId )
  {
    var projectName = await _versionService.GetProjectNameOfVersion( versionId );
    if ( projectName == null ) {
      return BadRequest( "Project not found." );
    }
    var groupTaskResources = await GetGroupTaskResourcesByProjectId( versionId );
    var bgResources = await GetBackgrounds( versionId );
    var usedColor = await GetUsedColor( versionId, groupTaskResources, bgResources );
    var setting = await _projectSettingService.GetProjectSetting( versionId );
    var viewResources = await GetViewTasks( versionId );
    if ( ExportExcel.ExportExcel.GetFile( projectName, setting!, usedColor, viewResources, groupTaskResources, bgResources, out var result ) ) {
      var fileBytes = System.IO.File.ReadAllBytes( result );
      return File( fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, $"{Guid.NewGuid()}.xlsx" );
    }
    else {
      return BadRequest( result );
    }
  }

  private async Task<IEnumerable<ColorDef>> GetUsedColor( long projectId, IEnumerable<GroupTaskDetailResource> tasks, IEnumerable<ProjectBackgroundResource> background )
  {
    var colorSetting = await _colorService.GetAllColorDefsByProjectId( projectId );
    var usedStepworkColorIds = tasks.SelectMany( x => x.Tasks ).SelectMany( x => x.Stepworks ).Select( x => x.ColorId );
    var usedBgColorIds = background.Select( x => x.ColorId );

    return colorSetting.Where( x => usedStepworkColorIds.Contains( x.ColorId ) || usedBgColorIds.Contains( x.ColorId ) );
  }

  private async Task<Dictionary<View, List<ViewTaskDetail>>> GetViewTasks( long projectId )
  {
    var result = new Dictionary<View, List<ViewTaskDetail>>();
    var views = await _viewService.GetViewsByProjectId( projectId );
    foreach ( var view in views ) {
      var viewTasks = await _viewService.GetViewTasks( projectId, view.ViewId );
      result.Add( view, viewTasks.ToList() );
    }
    return result;
  }

  private async Task<List<GroupTaskDetailResource>> GetGroupTaskResourcesByProjectId( long versionId )
  {
    var groupTasks = await _groupTaskService.GetGroupTasksByVersionId( versionId );
    var groupTaskResources = _mapper.Map<List<GroupTaskDetailResource>>( groupTasks );

    var stepworkColors = ( await _colorService.GetStepworkColorDefsByVersionId( versionId ) )
      .ToDictionary( x => x.ColorId, x =>
      new ColorDetailResource()
      {
        Name = x.Name,
        Code = x.Code,
        ColorId = x.ColorId,
        ColorMode =  x.IsInstall == 0 ? ColorMode.Install : ( x.IsInstall == 1 ? ColorMode.Removal : ColorMode.Custom ) 
      } );

    foreach ( var groupTaskResource in groupTaskResources ) {
      var tasks = await _taskService.GetTasksByGroupTaskId( groupTaskResource.GroupTaskId );
      groupTaskResource.Tasks = _mapper.Map<List<TaskDetailResource>>( tasks );
      foreach ( var taskResource in groupTaskResource.Tasks ) {
        var stepworks = await _stepworkService.GetStepworksByTaskId( taskResource.TaskId );
        taskResource.Stepworks = _mapper.Map<List<StepworkDetailResource>>( stepworks );
        foreach ( var stepworkResource in taskResource.Stepworks ) {
          var predecessor = await _predecessorService.GetPredecessorsByStepworkId( stepworkResource.StepworkId );
          stepworkResource.Predecessors = _mapper.Map<List<PredecessorDetailResource>>( predecessor );
          if ( stepworkColors.ContainsKey( stepworkResource.ColorId ) ) {
            stepworkResource.ColorDetail = stepworkColors [ stepworkResource.ColorId ];
          }
        }
      }
    }
    return groupTaskResources;
  }

  private async Task<IEnumerable<ProjectBackgroundResource>> GetBackgrounds( long versionId )
  {
    var colors = await _colorService.GetBackgroundColorDefsByVersionId( versionId );

    var backgrounds = await _backgroundService.GetBackgroundsByVersionId( versionId );
    var resources = _mapper.Map<IEnumerable<ProjectBackgroundResource>>( backgrounds );
    foreach ( var background in resources ) {
      if ( background.ColorId == null ) {
        continue;
      }
      var color = colors.FirstOrDefault( c => c.ColorId == background.ColorId );
      if ( color == null ) {
        continue;
      }
      background.ColorCode = color.Code;
      background.Name = color.Name;
    }
    return resources;
  }
}