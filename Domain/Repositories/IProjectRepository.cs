using SchedulingTool.Api.Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IProjectRepository : IGenericRepository<Project>
{
  Task<IEnumerable<Project>> GetActiveProjects( long userId );
  Task<IEnumerable<Project>> GetActiveProjects( long userId, ICollection<long> projectIds );
  Task BatchDeleteProjectDetails( long projectId );
}
