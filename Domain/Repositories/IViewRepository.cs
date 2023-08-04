using SchedulingTool.Api.Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IViewRepository : IGenericRepository<View>
{
  Task<IEnumerable<View>> GetViewsByVersionId( long versionId );
  Task DeleteView( long viewId, bool isDeleteView );
  Task<IEnumerable<ViewTaskDetail>> GetViewTasks( long versionId, long viewId );
}
