using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Persistence.Repositories;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Services;

public class ViewTaskService:IViewTaskService
{
  private readonly IViewTaskRepository _viewTaskRepository;
  private readonly IUnitOfWork _unitOfWork;

  public ViewTaskService( IViewTaskRepository viewTaskRepository, IUnitOfWork unitOfWork )
  {
    _viewTaskRepository = viewTaskRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IEnumerable<ViewTask>> GetViewTasksByViewId( long viewId )
  {
    return await _viewTaskRepository.GetViewTasksByViewId( viewId );
  }

  public async Task<ServiceResponse<ViewTask>> CreateViewTask( ViewTask viewTask )
  {
    try {
      var newViewTask = await _viewTaskRepository.Create( viewTask );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ViewTask>( newViewTask );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ViewTask>( $"An error occured when saving view task {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<ViewTask>> UpdateViewTask( ViewTask viewTask )
  {
    try {
      await _viewTaskRepository.Update( viewTask );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ViewTask>( viewTask );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ViewTask>( $"An error occured when saving view task {ex.Message}" );
    }
  }

  public async Task DeleteViewTasksByViewId( long viewId )
  {
    await _viewTaskRepository.DeleteViewTasksByViewId( viewId );
  }
}
