using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public class StepworkRepository : GenericRepository<Stepwork>, IStepworkRepository
{
  public StepworkRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<IEnumerable<Stepwork>>   GetStepworksByTaskId(long taskId )
  {
    return await _context.Stepworks.Where( sw => sw.TaskId == taskId ).ToListAsync();
  }
}
