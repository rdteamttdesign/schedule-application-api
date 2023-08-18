using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;

namespace SchedulingTool.Api.Persistence.Repositories;

public class TaskRepository : GenericRepository<ModelTask>, ITaskRepository
{
  public TaskRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<IEnumerable<ModelTask>> GetTasksByGroupTaskId( long groupTaskId )
  {
    return await _context.Tasks.Where( task => task.GroupTaskId == groupTaskId ).ToListAsync();
  }
}
