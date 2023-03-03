using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;

namespace SchedulingTool.Api.Domain.Services;

public interface IPredecessorService
{
  Task<IEnumerable<Predecessor>> GetPredecessorsByStepworkId( long stepworkId );
  Task<Predecessor?> GetPredecessor( long stepworkId, long relatedStepworkId );
  Task<ServiceResponse<Predecessor>> CreatePredecessor( Predecessor predecessors );
  Task<ServiceResponse<Predecessor>> UpdatePredecessor( Predecessor predecessors );
  Task<ServiceResponse<Predecessor>> DeletePredecessor( long stepworkId, long relatedSetpworkId );
}
