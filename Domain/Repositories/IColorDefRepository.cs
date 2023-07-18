using SchedulingTool.Api.Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IColorDefRepository : IGenericRepository<ColorDef>
{
  Task<IEnumerable<ColorDef>> GetBackgroundColorDefsByProjectId( long projectId );

  Task<IEnumerable<ColorDef>> GetStepworkColorDefsByProjectId( long projectId );

  Task<IEnumerable<ColorDef>> GeAllColorDefsByProjectId( long projectId );

  Task DuplicateColorDefs( long fromProjectId, long toProjectId );
}
