using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Services;

public interface IProjectService
{
  Task<ServiceResponse<Project>> CreateProject( Project project );
  Task<IEnumerable<Project>> GetActiveProjects( long userId );
  Task<Project?> GetProject( long userId, long projectId );
  Task BatchDeactiveProjects( long userId, ICollection<long> projectIds );
  Task<ServiceResponse<Project>> UpdateProject( Project project );
  Task BatchDeleteProjectDetails( long projectId );
}
