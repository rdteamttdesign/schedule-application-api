using AutoMapper;
using Microsoft.CodeAnalysis;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services.Unused;

namespace SchedulingTool.Api.Services.Unused;

public class ViewService : IViewService
{
  private readonly IViewRepository _viewRepository;
  private readonly IStepworkRepository _stepworkRepository;
  private readonly IProjectSettingRepository _projectSettingRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public ViewService( 
    IViewRepository viewRepository, 
    IUnitOfWork unitOfWork, 
    IMapper mapper, 
    IStepworkRepository stepworkRepository, 
    IProjectSettingRepository projectSettingRepository )
  {
    _viewRepository = viewRepository;
    _unitOfWork = unitOfWork;
    _mapper = mapper;
    _stepworkRepository = stepworkRepository;
    _projectSettingRepository = projectSettingRepository;
  }

  public async Task<IEnumerable<View>> GetViewsByProjectId( long projectId )
  {
    return await _viewRepository.GetViewsByVersionId( projectId );
  }

  public async Task<View?> GetViewById( long viewId )
  {
    return await _viewRepository.GetById( viewId );
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
          var gap = 0d;
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

  public async Task<IEnumerable<ViewTaskDetail>> GetViewTasks( long projectId, long viewId )
  {
    var setting = await _projectSettingRepository.GetByVersionId( projectId );
    var viewtasks = await _viewRepository.GetViewTasks( projectId, viewId );
    foreach ( var task in viewtasks ) {
      var stepworks = await _stepworkRepository.GetStepworksByTaskId( task.TaskId );
      task.Stepworks = stepworks.ToList();
    }
    CalculateDuration( viewtasks, setting! );
    return viewtasks;
  }
}
