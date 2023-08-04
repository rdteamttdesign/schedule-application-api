using AutoMapper;
using Microsoft.CodeAnalysis;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Services;

public class ViewService : IViewService
{
  private readonly IViewRepository _viewRepository;
  private readonly IStepworkRepository _stepworkService;
  private readonly IProjectSettingRepository _projectSettingRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public ViewService( 
    IViewRepository viewRepository, 
    IUnitOfWork unitOfWork, 
    IMapper mapper, 
    IStepworkRepository stepworkService, 
    IProjectSettingRepository projectSettingRepository )
  {
    _viewRepository = viewRepository;
    _unitOfWork = unitOfWork;
    _mapper = mapper;
    _stepworkService = stepworkService;
    _projectSettingRepository = projectSettingRepository;
  }

  public async Task<IEnumerable<View>> GetViewsByVersionId( long versionId )
  {
    return await _viewRepository.GetViewsByVersionId( versionId );
  }

  public async Task<View?> GetViewById( long viewId )
  {
    return await _viewRepository.GetById( viewId );
  }

  public async Task<IEnumerable<object>> GetViewDetailById( long projectId, IEnumerable<ViewTaskDetail> tasks )
  {
    var setting = await _projectSettingRepository.GetByVersionId( projectId );
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
        // not in group
        foreach ( var task in group ) {
          var minStart = task.Stepworks.Min( s => s.Start );
          var maxEnd = task.Stepworks.Max( s => s.End );
          task.MinStart = minStart;
          task.MaxEnd = maxEnd;
          var duration = task.MaxEnd - task.MinStart;
          if ( task.Stepworks.Count > 1 && task.NumberOfTeam != 0 ) {
            // more than one stepwork and number of teams > 0
            for ( int i = 0; i < task.Stepworks.Count - 1; i++ ) {
              duration += task.Stepworks.ElementAt( i ).Duration * ( setting!.AmplifiedFactor - 1 ) / task.NumberOfTeam;
            }
            task.Duration = duration;
          }
          else {
            // other cases
            task.Duration = duration;
          }
        }
      }
      else {
        // in group
        foreach ( var task in group ) {
          if ( !( task.Stepworks.Count > 1 && task.NumberOfTeam != 0 ) ) {
            continue;
          }
          //Recalculate end of last stepwork if task has more than one stepwork
          var gap = 0f;
          for ( int i = 0; i < task.Stepworks.Count - 1; i++ ) {
            gap += task.Stepworks.ElementAt( i ).Duration * ( setting!.AmplifiedFactor - 1 ) / task.NumberOfTeam;
          }
          task.Stepworks.Last().End += gap;
        }
        var stepworks = group.SelectMany( task => task.Stepworks );
        var minStart = stepworks.Min( s => s.Start );
        var maxEnd = stepworks.Max( s => s.End );
        var lastTask = group.First( t => t.Stepworks.Any( s => s.End == maxEnd ) );
        var duration = maxEnd - minStart;

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
