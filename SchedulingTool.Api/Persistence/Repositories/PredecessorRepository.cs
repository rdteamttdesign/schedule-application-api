using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public class PredecessorRepository : GenericRepository<Predecessor>, IPredecessorRepository
{
  public PredecessorRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<Predecessor?> GetById( long stepworkId, long relatedStepworkId )
  {
    return await _context.Predecessors.FindAsync( stepworkId, relatedStepworkId );
  }

  public async Task<IEnumerable<Predecessor>> GetPredecessorsByStepworkId( long stepworkId )
  {
    return await _context.Predecessors.Where( p => p.StepworkId == stepworkId ).ToListAsync();
  }
}
