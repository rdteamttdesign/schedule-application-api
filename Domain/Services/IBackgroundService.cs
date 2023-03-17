using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Services;

public interface IBackgroundService
{
  Task<ProjectBackground?> GetProjectBackground( long projectId, int month );
  Task<IEnumerable<ProjectBackground>> GetBackgroundsByProjectId( long projectId );
  Task BatchDelete( long projectId, int fromMonth );
  Task<ServiceResponse<ProjectBackground>> UpdateProjectBackground( ProjectBackground projectBackground );

  Task AddMonth( long projectId, int numberOfMonth );
}
