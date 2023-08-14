using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;

namespace SchedulingTool.Api.Services;

public class StepworkService : IStepworkService
{
  private readonly IStepworkRepository _stepworkRepository;
  private readonly IUnitOfWork _unitOfWork;

  public StepworkService( IStepworkRepository stepworkRepository, IUnitOfWork unitOfWork )
  {
    _stepworkRepository = stepworkRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IEnumerable<Stepwork>> GetStepworksByTaskId( long taskId )
  {
    return await _stepworkRepository.GetStepworksByTaskId( taskId );
  }

  public async Task<Stepwork?> GetStepworkById( long stepworkId )
  {
    return await _stepworkRepository.GetById( stepworkId );
  }

  public async Task<ServiceResponse<Stepwork>> CreateStepwork( Stepwork stepwork )
  {
    try {
      var newStepwork = await _stepworkRepository.Create( stepwork );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Stepwork>( newStepwork );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Stepwork>( $"{StepworkNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<Stepwork>> UpdateStepwork( Stepwork stepwork )
  {
    try {
      await _stepworkRepository.Update( stepwork );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Stepwork>( stepwork );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Stepwork>( $"{StepworkNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<Stepwork>> DeleteStepwork( long stepworkId )
  {
    try {
      var stepwork = await _stepworkRepository.GetById( stepworkId );
      if ( stepwork is null )
        return new ServiceResponse<Stepwork>( StepworkNotification.NonExisted );
      _stepworkRepository.Delete( stepwork );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Stepwork>( stepwork );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Stepwork>( $"{StepworkNotification.ErrorDeleting} {ex.Message}" );
    }
  }
}
