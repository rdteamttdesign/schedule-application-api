using SchedulingTool.Api.Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IColorDefRepository : IGenericRepository<ColorDef>
{
  Task<IEnumerable<ColorDef>> GetBackgroundColorDefsByVersionId( long versionId );

  Task<IEnumerable<ColorDef>> GetStepworkColorDefsByVersionId( long versionId );

  Task DuplicateColorDefs( long fromVersionId, long toVersionId );
}
