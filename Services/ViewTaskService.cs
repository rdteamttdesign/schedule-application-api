using AutoMapper;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Services;

public class ViewTaskService : IViewTaskService
{
  private readonly IViewTaskRepository _viewTaskRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public ViewTaskService( IViewTaskRepository viewTaskRepository, IUnitOfWork unitOfWork, IMapper mapper )
  {
    _viewTaskRepository = viewTaskRepository;
    _unitOfWork = unitOfWork;
    _mapper = mapper;
  }

  public async Task<IEnumerable<ViewTask>> GetViewTasksByViewId( long viewId )
  {
    return await _viewTaskRepository.GetViewTasksByViewId( viewId );
  }

  public async Task<ServiceResponse<ICollection<ViewTaskResource>>> CreateViewTasks( long viewId, ICollection<ViewTaskFormData> viewTasks )
  {
    try {
      List<ViewTaskResource> result = new();
      foreach ( var item in viewTasks ) {
        var viewTask = new ViewTask()
        {
          ViewId = viewId,
          LocalTaskId = item.Id,
          Group = item.Group,
          DisplayOrder = item.DisplayOrder
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
