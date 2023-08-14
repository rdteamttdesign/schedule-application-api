using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IPredecessorRepository : IGenericRepository<Predecessor>
{
  Task<Predecessor?> GetById( long stepworkId, long relatedStepworkId );
  Task<IEnumerable<Predecessor>> GetPredecessorsByStepworkId( long stepworkId );
}
