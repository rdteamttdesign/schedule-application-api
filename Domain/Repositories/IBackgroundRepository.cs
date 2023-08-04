using SchedulingTool.Api.Domain.Models;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IBackgroundRepository : IGenericRepository<ProjectBackground>
{
  Task<ProjectBackground?> GetProjectBackground( long versionId, int month );
  Task<IEnumerable<ProjectBackground>> GetBackgroundsByVersionId( long versionId );
  Task BatchDelete( long versionId, int fromMonth );
  Task AddMonth( long versionId, int numberOfMonth );
}
