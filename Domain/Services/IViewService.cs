using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Resources;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Services;

public interface IViewService
{
  Task<IEnumerable<View>> GetViewsByProjectId( long projectId );
  Task<ServiceResponse<View>> CreateView( View view );
  Task<ServiceResponse<View>> UpdateView( View view );
  Task<View?> GetViewById( long viewId );
  Task DeleteView( long viewId );
  Task<IEnumerable<ViewTaskResource>> GetViewTasks( long viewId );
}
