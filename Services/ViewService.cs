using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Persistence.Repositories;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Services;

public class ViewService : IViewService
{
  private readonly IViewRepository _viewRepository;
  private readonly IUnitOfWork _unitOfWork;

  public ViewService( IViewRepository viewRepository, IUnitOfWork unitOfWork )
  {
    _viewRepository = viewRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IEnumerable<View>> GetViewsByProjectId( long projectId )
  {
    return await _viewRepository.GetViewsByProjectId( projectId );
  }

  public async Task<View?> GetViewById( long viewId )
  {
    return await _viewRepository.GetById( viewId );
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

  public async Task DeleteView( long viewId )
  {
    _viewRepository.DeleteView( viewId );
  }
}
