using SchedulingTool.Api.Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IColorDefRepository : IGenericRepository<ColorDef>
{
  Task<IEnumerable<ColorDef>> GetBackgroundColorsByVersionId( long versionId );

  Task<IEnumerable<ColorDef>> GetStepworkColorsByProjectId( long versionId );

  Task<IEnumerable<ColorDef>> GeAllColorsByVersionId( long versionId );
}
