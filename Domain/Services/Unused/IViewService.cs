using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Services.Unused;

public interface IViewService
{
  Task<IEnumerable<View>> GetViewsByProjectId( long projectId );
  Task<View?> GetViewById( long viewId );
  Task<IEnumerable<ViewTaskDetail>> GetViewTasks( long projectId, long viewId );
}
