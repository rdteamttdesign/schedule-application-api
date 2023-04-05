using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Resources;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IViewRepository : IGenericRepository<View>
{
  Task<IEnumerable<View>> GetViewsByProjectId( long projectId );
  Task DeleteView( long viewId );
  Task<IEnumerable<ViewTaskResource>> GetViewTasks( long viewId );
}
