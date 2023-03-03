using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;

namespace SchedulingTool.Api.Services;

public class TaskService : ITaskService
{
  private readonly ITaskRepository _taskRepository;
  private readonly IUnitOfWork _unitOfWork;

  public TaskService( ITaskRepository taskRepository, IUnitOfWork unitOfWork )
  {
    _taskRepository = taskRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IEnumerable<ModelTask>> GetTasksByGroupTaskId( long groupTaskId )
  {
    return await _taskRepository.GetTasksByGroupTaskId( groupTaskId );
  }

  public async Task<ModelTask?> GetTaskById( long taskId )
  {
    return await _taskRepository.GetById( taskId );
  }

  public async Task<ServiceResponse<ModelTask>> CreateTask( ModelTask task )
  {
    try {
      var newTask = await _taskRepository.Create( task );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ModelTask>( newTask );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ModelTask>( $"{TaskNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<ModelTask>> UpdateTask( ModelTask task )
  {
    try {
      await _taskRepository.Update( task );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ModelTask>( task );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ModelTask>( $"{TaskNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<ModelTask>> DeleteTask( long taskId )
  {
    try {
      var task = await _taskRepository.GetById( taskId );
      if ( task is null )
        return new ServiceResponse<ModelTask>( TaskNotification.NonExisted );
      _taskRepository.Delete( task );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ModelTask>( task );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ModelTask>( $"{TaskNotification.ErrorDeleting} {ex.Message}" );
    }
  }
}
