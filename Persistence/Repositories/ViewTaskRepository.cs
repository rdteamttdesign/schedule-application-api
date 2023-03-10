using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ViewTaskRepository : GenericRepository<ViewTask>, IViewTaskRepository
{
  public ViewTaskRepository( AppDbContext context ) : base( context )
  {

  }

  public async Task<IEnumerable<ViewTask>> GetViewTasksByViewId( long viewId )
  {
    return await _context.ViewTasks.Where( x => x.ViewId == viewId ).ToListAsync();
  }

  public async Task<ViewTask?> GetViewTask( long viewId, long taskId )
  {
    return await _context.ViewTasks.FindAsync( viewId, taskId );
  }

  public async Task DeleteViewTasksByViewId( long viewId )
  {
    await _context.Database.ExecuteSqlRawAsync( "", viewId );
  }
}
