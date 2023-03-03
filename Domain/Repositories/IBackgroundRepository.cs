using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IBackgroundRepository : IGenericRepository<ProjectBackground>
{
  Task<ProjectBackground?> GetBackground( long projectId, int month );
  Task<IEnumerable<ProjectBackground>> GetBackgroundsByProjectId( long projectId );
}
