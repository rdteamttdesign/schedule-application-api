using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using Task = System.Threading.Tasks.Task;
using Version = SchedulingTool.Api.Domain.Models.Version;

namespace SchedulingTool.Api.Domain.Services;

public interface IVersionService
{
  Task<ServiceResponse<Version>> CreateVersion( long projectId, Version version );
  Task<IEnumerable<Version>> GetActiveVersions( long userId, long projectId );
  Task<Version?> GetVersionById( long versionId );
  Task BatchDeactiveVersions( long userId, ICollection<long> versionIds );
  Task<ServiceResponse<Version>> UpdateVersion( Version version );
  Task BatchDeleteVersionDetails( long versionId );
  Task BatchDeleteVersions( ICollection<long> versionIds );
  Task BatchActivateVersions( long userId, ICollection<long> versionIds );
  Task<IEnumerable<object>> GetGroupTasksByVersionId( long versionId, int columnWidth, float amplifiedFactor );
  Task SaveProjectTasks( long versionId, ICollection<CommonGroupTaskFormData> formData );
  Task<ServiceResponse<VersionResource>> DuplicateProject( long projectId, Version oldVersion );
  Task<dynamic> GetDataFromFile( long versionId, Stream fileStream, int maxDisplayOrder, string [] sheetNameList );
  Task<dynamic> GetUpdatedDataFromFile( long versionId, Stream fileStream, string [] sheetNameList );
}
