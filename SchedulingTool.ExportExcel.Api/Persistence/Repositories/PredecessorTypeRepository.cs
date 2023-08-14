using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public class PredecessorTypeRepository : GenericRepository<PredecessorType>, IPredecessorTypeRepository
{
  public PredecessorTypeRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<IEnumerable<PredecessorType>> GetAll()
  {
    return await _context.PredecessorTypes.ToListAsync();
  }
}
