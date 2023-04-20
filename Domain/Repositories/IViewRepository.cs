using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Resources;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IViewRepository : IGenericRepository<View>
{
  Task<IEnumerable<View>> GetViewsByProjectId( long projectId );
  Task DeleteView( long viewId, bool isDeleteView );
  Task<IEnumerable<ViewTaskDetail>> GetViewTasks( long projectId, long viewId );
}
