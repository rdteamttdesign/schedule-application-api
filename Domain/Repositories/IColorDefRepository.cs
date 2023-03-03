using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IColorDefRepository : IGenericRepository<ColorDef>
{
  Task<IEnumerable<ColorDef>> GetBackgroundColorDefsByProjectId( long projectId );

  Task<IEnumerable<ColorDef>> GetStepworkColorDefsByProjectId( long projectId );
}
