using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Services;

public interface IViewTaskService
{
  Task<IEnumerable<ViewTask>> GetViewTasksByViewId( long viewId );
  Task<ServiceResponse<ViewTask>> CreateViewTask( ViewTask viewTask );
  Task DeleteViewTasksByViewId( long viewId );
}
