using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Domain.Services.Unused;
using SchedulingTool.Api.Notification;

namespace SchedulingTool.Api.Services.Unused;

public class BackgroundService : IBackgroundService
{
  private readonly IBackgroundRepository _backgroundRepository;
  private readonly IUnitOfWork _unitOfWork;

  public BackgroundService( IBackgroundRepository backgroundRepository, IUnitOfWork unitOfWork )
  {
    _backgroundRepository = backgroundRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IEnumerable<ProjectBackground>> GetBackgroundsByVersionId( long projectId )
  {
    return await _backgroundRepository.GetBackgroundsByVersionId( projectId );
  }

  //public async Task BatchDelete( long projectId, int fromMonth )
  //{
  //  await _backgroundRepository.BatchDelete( projectId, fromMonth );
  //}

  //public async Task AddMonth( long projectId, int numberOfMonth )
  //{
  //  await _backgroundRepository.AddMonth( projectId, numberOfMonth );
  //}

  //public async Task<ProjectBackground?> GetProjectBackground( long projectId, int month )
  //{
  //  return await _backgroundRepository.GetProjectBackground( projectId, month );
  //}

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
