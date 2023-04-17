using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Services;

public interface IViewTaskService
{
  Task<IEnumerable<ViewTask>> GetViewTasksByViewId( long viewId );
  Task<ServiceResponse<ICollection<ViewTaskResource>>> CreateViewTasks( long viewId, ICollection<ViewTaskFormData> viewTask );
  Task DeleteViewTasksByViewId( long viewId );
}
