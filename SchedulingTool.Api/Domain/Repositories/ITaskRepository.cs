using ModelTask = SchedulingTool.Api.Domain.Models.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface ITaskRepository : IGenericRepository<ModelTask>
{
  Task<IEnumerable<ModelTask>> GetTasksByGroupTaskId( long groupTaskId );
}
