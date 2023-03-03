using SchedulingTool.Api.Domain.Services.Communication;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;

namespace SchedulingTool.Api.Domain.Services;
public interface ITaskService
{
  Task<IEnumerable<ModelTask>> GetTasksByGroupTaskId( long groupTaskId );
  Task<ModelTask?> GetTaskById( long taskId );
  Task<ServiceResponse<ModelTask>> CreateTask( ModelTask task );
  Task<ServiceResponse<ModelTask>> UpdateTask( ModelTask task );
  Task<ServiceResponse<ModelTask>> DeleteTask( long taskId );
}
