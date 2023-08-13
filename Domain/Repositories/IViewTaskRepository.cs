using SchedulingTool.Api.Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IViewTaskRepository : IGenericRepository<ViewTask>
{
  Task<IEnumerable<ViewTask>> GetViewTasksByViewId( long viewId );
  Task DeleteViewTasksByViewId( long viewId );
}
