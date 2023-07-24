using Task = System.Threading.Tasks.Task;
using Version = SchedulingTool.Api.Domain.Models.Version;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IVersionRepository : IGenericRepository<Version>
{
  Task<IEnumerable<Version>> GetActiveVersions( long userId );
  Task<IEnumerable<Version>> GetActiveVersions( long userId, ICollection<long> versionIds );
  Task BatchDeleteVersionDetails( long versionId );
}
