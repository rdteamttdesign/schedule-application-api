using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Resources;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IViewRepository : IGenericRepository<View>
{
  Task<IEnumerable<View>> GetViewsByVersionId( long versionId );
  Task<IEnumerable<ViewTaskDetail>> GetViewTasks( long versionId, long viewId );
}
