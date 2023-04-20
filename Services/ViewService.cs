using AutoMapper;
using Microsoft.CodeAnalysis;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources.projectdetail;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Services;

public class ViewService : IViewService
{
  private readonly IViewRepository _viewRepository;
  private readonly IStepworkRepository _stepworkService;
  private readonly IProjectSettingRepository _projectSettingRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public ViewService( IViewRepository viewRepository, IUnitOfWork unitOfWork, IMapper mapper, IStepworkRepository stepworkService, IProjectSettingRepository projectSettingRepository )
  {
    _viewRepository = viewRepository;
    _unitOfWork = unitOfWork;
    _mapper = mapper;
    _stepworkService = stepworkService;
    _projectSettingRepository = projectSettingRepository;
  }

  public async Task<IEnumerable<View>> GetViewsByProjectId( long projectId )
  {
    return await _viewRepository.GetViewsByProjectId( projectId );
  }

  public async Task<View?> GetViewById( long viewId )
  {
    return await _viewRepository.GetById( viewId );
  }

  public async Task<IEnumerable<object>> GetViewDetailById( View view, IEnumerable<GroupTask> groupTasks, List<ModelTask> tasks )
  {
    var result = new List<KeyValuePair<int, object>>();
    // get group task
    var groupTaskResources = _mapper.Map<List<GroupTaskResource>>( groupTasks );
    groupTaskResources.ForEach( g => result.Add( new KeyValuePair<int, object>( g.DisplayOrder, g ) ) );
    // get project setting
    var projectSetting = await _projectSettingRepository.GetByProjectId( view.ProjectId );

    // get task
    IEnumerable<IGrouping<int?, ModelTask>> groupViewTasks = tasks.GroupBy( s => s.Group );
    foreach ( IGrouping<int?, ModelTask> group in groupViewTasks ) {
      if ( group.Count() == 1 || group.Key is null || group.Key == 0 ) {
        foreach ( var task in group ) {
          var stepworks = await _stepworkService.GetStepworksByTaskLocalId( group.First().LocalId );
          var taskResource = _mapper.Map<TaskResource>( group.First() );
          if ( stepworks.Count() == 1 ) {
            taskResource.Predecessors = null;
            taskResource.ColorId = stepworks.First().ColorId;
            taskResource.Start = stepworks.First().Start;
          }
          else {
            taskResource.Predecessors = null;
            taskResource.ColorId = stepworks.First().ColorId;
            taskResource.Start = stepworks.Min( s => s.Start );
            taskResource.Duration = ( float ) ( stepworks.Max( s => s.Start + s.Duration * projectSetting!.ColumnWidth / 30 ) - taskResource.Start ) * 30 / projectSetting!.ColumnWidth;
          }
          result.Add( new KeyValuePair<int, object>( taskResource.DisplayOrder, taskResource ) );
        }
      }
      else {
        List<Stepwork> stepworkGroup = new();
        List<TaskResource> taskGroup = new();
        foreach ( var task in group ) {
          var stepworks = await _stepworkService.GetStepworksByTaskLocalId( task.LocalId );
          stepworkGroup.AddRange( stepworks );
          var _ = _mapper.Map<TaskResource>( task );
          _.ColorId = stepworks.First().ColorId;
          taskGroup.Add( _mapper.Map<TaskResource>( task ) );
        }
        var start = stepworkGroup.Count() == 1 ? stepworkGroup.First().Start : stepworkGroup.Min( x => x.Start );
        var duration = stepworkGroup.Count() == 1 ? stepworkGroup.First().Duration : ( float ) ( stepworkGroup.Max( s => s.Start + s.Duration * projectSetting!.ColumnWidth / 30 ) - start ) * 30 / projectSetting!.ColumnWidth;
        taskGroup.ForEach( s => { s.Start = start; s.Duration = duration; } );
        result.AddRange( taskGroup.Select( s => new KeyValuePair<int, object>( s.DisplayOrder, s ) ) );
      }
    }
    return result.OrderBy( o => o.Key ).Select( o => o.Value );
  }

  public async Task<IEnumerable<object>> GetViewDetailById( long projectId, IEnumerable<ViewTaskDetail> tasks )
  {
    var setting = await _projectSettingRepository.GetByProjectId( projectId );
    CalculateDuration( tasks, setting! );
    var result = new List<KeyValuePair<int, object>>();
    var tasksByGroup = tasks.GroupBy( t => t.GroupTaskName );

    int i = 1;
    foreach ( var group in tasksByGroup ) {
      var groupId = Guid.NewGuid().ToString();
      var grouptaskResource = new GroupTaskResource()
      {
        Name = group.Key,
        Id = groupId,
        Type = "project",
        DisplayOrder = i
      };
      result.Add( new KeyValuePair<int, object>( i, grouptaskResource ) );
      foreach ( var task in group ) {
        var taskResource = new TaskResource()
        {
          Duration = task.Duration,
          Start = task.MinStart.DaysToColumnWidth( setting!.ColumnWidth ),
          End = task.MaxEnd.DaysToColumnWidth( setting.ColumnWidth ),
          Name = task.TaskName,
          Id = task.TaskLocalId,
          Type = "task",
          Detail = string.Empty,
          GroupId = groupId,
          DisplayOrder = i,
          Note = string.Empty,
          ColorId = 1,
          GroupsNumber = 0
        };
        i++;
        result.Add( new KeyValuePair<int, object>( i, taskResource ) );
      }
      i++;
    }
    return result.OrderBy( o => o.Key ).Select( o => o.Value );
  }

  private void CalculateDuration( IEnumerable<ViewTaskDetail> tasks, ProjectSetting setting )
  {
    var tasksByGroup = tasks.GroupBy( t => t.Group );
    foreach ( var group in tasksByGroup ) {
      if ( group.Count() < 1 ) {
        continue;
      }
      if ( group.Key == 0 ) {
        foreach ( var task in group ) {
          var minStart = task.Stepworks.Min( s => s.Start );
          var maxEnd = task.Stepworks.Max( s => s.End );
          task.MinStart = minStart;
          task.MaxEnd = maxEnd;
          task.Duration = task.MaxEnd - task.MinStart + ( task.NumberOfTeam == 0 ? 0 : ( task.Duration * ( setting!.AmplifiedFactor - 1 ) ) );
        }
      }
      else {
        var stepworks = group.SelectMany( task => task.Stepworks );
        var minStart = stepworks.Min( s => s.Start );
        var maxEnd = stepworks.Max( s => s.End );
        var lastTask = group.First( t => t.Stepworks.Any( s => s.End == maxEnd ) );
        var duration = maxEnd - minStart + ( lastTask.NumberOfTeam == 0 ? 0 : ( lastTask.Duration * ( setting!.AmplifiedFactor - 1 ) ) );

        foreach ( var task in group ) {
          task.MinStart = minStart;
          task.MaxEnd = maxEnd;
          task.Duration = duration;
        }
      }
    }
  }

  public async Task<ServiceResponse<View>> CreateView( View view )
  {
    try {
      var newTask = await _viewRepository.Create( view );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<View>( newTask );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<View>( $"{ViewNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<View>> UpdateView( View view )
  {
    try {
      await _viewRepository.Update( view );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<View>( view );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<View>( $"{ViewNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task DeleteView( long viewId, bool isDeleteView )
  {
    await _viewRepository.DeleteView( viewId, isDeleteView );
  }

  public async Task<IEnumerable<ViewTaskDetail>> GetViewTasks( long projectId, long viewId )
  {
    return await _viewRepository.GetViewTasks( projectId, viewId );
  }
}
