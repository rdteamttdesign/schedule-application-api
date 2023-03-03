using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;

namespace SchedulingTool.Api.Services;

public class PredecessorService : IPredecessorService
{
  private readonly IPredecessorRepository _predecessorRepository;
  private readonly IUnitOfWork _unitOfWork;

  public PredecessorService( IPredecessorRepository predecessorRepository, IUnitOfWork unitOfWork )
  {
    _predecessorRepository = predecessorRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IEnumerable<Predecessor>> GetPredecessorsByStepworkId( long stepworkId )
  {
    return await _predecessorRepository.GetPredecessorsByStepworkId( stepworkId );
  }

  public async Task<Predecessor?> GetPredecessor( long stepworkId, long relatedStepworkId )
  {
    return await _predecessorRepository.GetById( stepworkId, relatedStepworkId );
  }

  public async Task<ServiceResponse<Predecessor>> CreatePredecessor( Predecessor predecessors )
  {
    try {
      var newPredecessor = await _predecessorRepository.Create( predecessors );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Predecessor>( newPredecessor );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Predecessor>( $"{PredecessorNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<Predecessor>> UpdatePredecessor( Predecessor predecessors )
  {
    try {
      await _predecessorRepository.Update( predecessors );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Predecessor>( predecessors );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Predecessor>( $"{PredecessorNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<Predecessor>> DeletePredecessor( long stepworkId, long relatedSetpworkId )
  {
    try {
      var stepwork = await _predecessorRepository.GetById( stepworkId, relatedSetpworkId );
      if ( stepwork is null )
        return new ServiceResponse<Predecessor>( PredecessorNotification.NonExisted );
      _predecessorRepository.Delete( stepwork );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Predecessor>( stepwork );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Predecessor>( $"{PredecessorNotification.ErrorDeleting} {ex.Message}" );
    }
  }
}
