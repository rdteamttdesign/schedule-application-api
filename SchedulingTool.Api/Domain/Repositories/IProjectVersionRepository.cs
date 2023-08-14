using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IProjectVersionRepository : IGenericRepository<ProjectVersion>
{
  Task<IEnumerable<ProjectVersion>> GetByProjectId( long projectId );

  Task<IEnumerable<ProjectVersion>> GetAll();
}
