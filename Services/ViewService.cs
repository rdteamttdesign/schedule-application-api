using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.Extended;
using SchedulingTool.Api.Resources.FormBody;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Services;

public class ViewService : IViewService
{
  private readonly IViewRepository _viewRepository;
  private readonly IGroupTaskRepository _groupTaskRepository;
  private readonly ITaskRepository _taskRepository;
  private readonly IViewTaskRepository _viewTaskRepository;
  private readonly IStepworkRepository _stepworkService;
  private readonly IProjectSettingRepository _projectSettingRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public ViewService(
    IViewRepository viewRepository,
    IGroupTaskRepository groupTaskRepository,
    ITaskRepository taskRepository,
    IViewTaskRepository viewTaskRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IStepworkRepository stepworkService,
    IProjectSettingRepository projectSettingRepository )
  {
    _viewRepository = viewRepository;
    _groupTaskRepository = groupTaskRepository;
    _taskRepository = taskRepository;
    _viewTaskRepository = viewTaskRepository;
    _unitOfWork = unitOfWork;
    _mapper = mapper;
    _stepworkService = stepworkService;
    _projectSettingRepository = projectSettingRepository;
  }

  public async Task<IEnumerable<View>> GetViewsByVersionId( long versionId )
  {
    return await _viewRepository.GetViewsByVersionId( versionId );
  }

  public async Task<ServiceResponse<IEnumerable<object>>> GetViewDetailById( long versionId, long viewId )
  {
    var view = await _viewRepository.GetById( viewId );
    if ( view == null ) {
      return new ServiceResponse<IEnumerable<object>>( ViewNotification.NonExisted );
    }
    var setting = await _projectSettingRepository.GetByVersionId( versionId );
    var viewTasks = await _viewRepository.GetViewTasks( versionId, viewId );
    if ( !viewTasks.Any() ) {
      return new ServiceResponse<IEnumerable<object>>( Array.Empty<object>() );
    }
    foreach ( var viewTask in viewTasks ) {
      var stepworks = await _stepworkService.GetStepworksByTaskId( viewTask.TaskId );
      viewTask.Stepworks = stepworks.ToList();
    }
    var tasks = viewTasks.OrderBy( t => t.DisplayOrder );
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
        var taskResource = new ViewTaskDetailResource()
        {
          Duration = task.Duration,
          Start = task.MinStart.DaysToColumnWidth( setting!.ColumnWidth ),
          End = task.MaxEnd.DaysToColumnWidth( setting.ColumnWidth ),
          Name = task.TaskName,
          Id = task.TaskLocalId,
          Type = "task",
          Detail = task.Description ?? string.Empty,
          GroupId = groupId,
          DisplayOrder = i,
          Note = task.Note ?? string.Empty,
          ColorId = 1,
          GroupsNumber = 0,
          IsHidden = task.IsHidden
        };
        i++;
        result.Add( new KeyValuePair<int, object>( i, taskResource ) );
      }
      i++;
    }
    return new ServiceResponse<IEnumerable<object>>( result.OrderBy( o => o.Key ).Select( o => o.Value ) );
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

  public async Task<ServiceResponse<ICollection<ViewTaskResource>>> CreateViewTasks( long versionId, long viewId, ICollection<ViewTaskFormData> viewTasks )
  {
    try {
      var grouptasks = await _groupTaskRepository.GetGroupTasksByVersionId( versionId );
      var tasks = new List<ModelTask>();
      foreach ( var groupTask in grouptasks ) {
        var _ = await _taskRepository.GetTasksByGroupTaskId( groupTask.GroupTaskId );
        tasks.AddRange( _ );
      }

      List<ViewTaskResource> result = new();
      foreach ( var item in viewTasks ) {
        var task = tasks.FirstOrDefault( x => x.LocalId == item.Id );
        var viewTask = new ViewTask()
        {
          ViewId = viewId,
          LocalTaskId = item.Id,
          Group = item.Group,
          DisplayOrder = item.DisplayOrder,
          IsHidden = false,
          TaskName = task.TaskName,
          TaskDescription = task?.Description,
          TaskNote = task?.Note
        };
        var viewTaskResult = await _viewTaskRepository.Create( viewTask );
        result.Add( _mapper.Map<ViewTaskResource>( viewTaskResult ) );
      }
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ICollection<ViewTaskResource>>( result );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ICollection<ViewTaskResource>>( $"An error occured when saving view task {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<ViewResource>> CreateView( long versionId, ViewFormData formData )
  {
    try {
      var view = new View()
      {
        ViewName = formData.ViewName,
        VersionId = versionId
      };
      var newTask = await _viewRepository.Create( view );
      await _unitOfWork.CompleteAsync();
      var resource = _mapper.Map<ViewResource>( view );

      var viewTaskResult = await CreateViewTasks( versionId, view.ViewId, formData.Tasks );
      if ( !viewTaskResult.Success ) {
        return new ServiceResponse<ViewResource>( viewTaskResult.Message );
      }
      resource.ViewTasks = viewTaskResult.Content;
      return new ServiceResponse<ViewResource>( resource );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ViewResource>( $"{ViewNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<ViewResource>> UpdateView( long versionId, long viewId, [FromBody] ViewFormData formData )
  {
    try {
      var view = await _viewRepository.GetById( viewId );
      if ( view == null ) {
        return new ServiceResponse<ViewResource>( ViewNotification.NonExisted );
      }
      view.ViewName = formData.ViewName;
      await _viewRepository.Update( view );
      await _unitOfWork.CompleteAsync();

      await DeleteView( viewId, false );
      await CreateViewTasks( versionId, viewId, formData.Tasks );

      var resource = _mapper.Map<ViewResource>( view );
      return new ServiceResponse<ViewResource>( resource );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ViewResource>( $"{ViewNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task DeleteView( long viewId, bool isDeleteView )
  {
    await _viewRepository.DeleteView( viewId, isDeleteView );
  }

  public async Task<ServiceResponse<View>> SaveViewDetail( long viewId, ICollection<ViewTaskDetailFormData> formData )
  {
    var view = await _viewRepository.GetById( viewId );
    if ( view == null ) {
      return new ServiceResponse<View>( ViewNotification.NonExisted );
    }
    var tasks = await _viewTaskRepository.GetViewTasksByViewId( viewId );
    foreach ( var task in formData ) {
      if ( task.Type != "task" ) {
        continue;
      }
      var existingTask = tasks.FirstOrDefault( x => x.LocalTaskId == task.Id );
      if ( existingTask == null ) {
        continue;
      }
      existingTask.TaskName = task.Name ?? string.Empty;
      existingTask.IsHidden = task.IsHidden;
      existingTask.TaskDescription = task.Detail;
      existingTask.TaskNote = task.Note;
      await _viewTaskRepository.Update( existingTask );
    }
    await _unitOfWork.CompleteAsync();

    return new ServiceResponse<View>( view );
  }

  public async Task DuplicateView( long fromVersionId, long toVersionId )
  {
    var existingViews = await _viewRepository.GetViewsByVersionId( fromVersionId );
    foreach ( var existingView in existingViews ) {
      var newView = new View()
      {
        ViewName = existingView.ViewName,
        VersionId = toVersionId
      };
      var newViewResult = await _viewRepository.Create( newView );
      if ( newViewResult == null ) {
        continue;
      }
      var viewtasks = await _viewTaskRepository.GetViewTasksByViewId( existingView.ViewId );
      foreach ( var task in viewtasks ) {
        var newViewTask = new ViewTask()
        {
          LocalTaskId = task.LocalTaskId,
          Group = task.Group,
          DisplayOrder = task.DisplayOrder,
          ViewId = newView.ViewId,
          IsHidden = task.IsHidden,
          TaskName = task.TaskName,
          TaskDescription = task.TaskDescription,
          TaskNote = task.TaskNote
        };
        await _viewTaskRepository.Create( newViewTask );
      }
      await _unitOfWork.CompleteAsync();
    }
  }
}
