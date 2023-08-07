using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Persistence.Repositories;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Services;

public class BackgroundService : IBackgroundService
{
  private readonly IBackgroundRepository _backgroundRepository;
  private readonly IUnitOfWork _unitOfWork;

  public BackgroundService( IBackgroundRepository backgroundRepository, IUnitOfWork unitOfWork )
  {
    _backgroundRepository = backgroundRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IEnumerable<ProjectBackground>> GetBackgroundsByVersionId( long versionId )
  {
    return await _backgroundRepository.GetBackgroundsByVersionId( versionId );
  }

  public async Task BatchDelete( long versionId, int fromMonth )
  {
    await _backgroundRepository.BatchDelete( versionId, fromMonth );
  }

  public async Task AddMonth( long versionId, int numberOfMonth )
  {
    await _backgroundRepository.AddMonth( versionId, numberOfMonth );
  }

  public async Task<ProjectBackground?> GetProjectBackground( long versionId, int month )
  {
    return await _backgroundRepository.GetProjectBackground( versionId, month );
  }

  public async Task<ServiceResponse<ProjectBackground>> UpdateProjectBackground( ProjectBackground projectBackground )
  {
    try {
      await _backgroundRepository.Update( projectBackground );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ProjectBackground>( projectBackground );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ProjectBackground>( $"{BackgroundNotification.ErrorSaving} {ex.Message}" );
    }
  }
}
