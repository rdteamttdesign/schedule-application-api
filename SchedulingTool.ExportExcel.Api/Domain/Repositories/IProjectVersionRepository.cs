using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IProjectVersionRepository : IGenericRepository<ProjectVersion>
{
  Task<ProjectVersion?> GetProjectVersionByVersionId( long versionId );
}
