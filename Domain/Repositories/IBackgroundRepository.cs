using SchedulingTool.Api.Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IBackgroundRepository : IGenericRepository<ProjectBackground>
{
  Task<ProjectBackground?> GetProjectBackground( long projectId, int month );
  Task<IEnumerable<ProjectBackground>> GetBackgroundsByProjectId( long projectId );
  Task BatchDelete( long projectId, int fromMonth );
  Task AddMonth( long projectId, int numberOfMonth );
}
