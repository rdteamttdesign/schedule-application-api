using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Services;

public interface IBackgroundService
{
  Task<ProjectBackground?> GetProjectBackground( long versionId, int month );
  Task<IEnumerable<ProjectBackground>> GetBackgroundsByVersionId( long versionId );
  Task BatchDelete( long versionId, int fromMonth );
  Task<ServiceResponse<ProjectBackground>> UpdateProjectBackground( ProjectBackground projectBackground );

  Task AddMonth( long versionId, int numberOfMonth );
}
