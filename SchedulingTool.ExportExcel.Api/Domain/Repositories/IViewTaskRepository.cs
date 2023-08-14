using SchedulingTool.Api.Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IViewTaskRepository : IGenericRepository<ViewTask>
{
  Task<IEnumerable<ViewTask>> GetViewTasksByViewId( long viewId );
  Task<ViewTask?> GetViewTask( long viewId, long taskId );
  Task DeleteViewTasksByViewId( long viewId );
}
