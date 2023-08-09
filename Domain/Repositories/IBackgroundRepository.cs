using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IBackgroundRepository : IGenericRepository<ProjectBackground>
{
  Task<IEnumerable<ProjectBackground>> GetBackgroundsByVersionId( long versionId );
}
