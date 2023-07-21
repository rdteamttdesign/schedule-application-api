using SchedulingTool.Api.Domain.Services.Communication;
using Task = System.Threading.Tasks.Task;
using Version = SchedulingTool.Api.Domain.Models.Version;

namespace SchedulingTool.Api.Domain.Services;

public interface IVersionService
{
  Task<ServiceResponse<Version>> CreateVersion( Version version );
  Task<IEnumerable<Version>> GetActiveVersions( long userId );
  Task<Version?> GetVersionById( long versionId );
  Task BatchDeactiveVersions( long userId, ICollection<long> versionIds );
  Task<ServiceResponse<Version>> UpdateVersion( Version version );
  Task BatchDeleteVersionDetails( long versionId );
}
