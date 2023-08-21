using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using Task = System.Threading.Tasks.Task;
using Version = SchedulingTool.Api.Domain.Models.Version;

namespace SchedulingTool.Api.Domain.Services;

public interface IVersionService
{
  Task<ServiceResponse<Version>> CreateVersion( long projectId, Version version );
  //Task<IEnumerable<Version>> GetActiveVersions( long userId, long projectId );
  Task<Version?> GetVersionById( long versionId );
  Task BatchDeactiveVersions( ICollection<long> versionIds, string userName );
  Task<ServiceResponse<Version>> UpdateVersion( Version version );
  Task BatchDeleteVersionDetails( long versionId );
  Task BatchDeleteVersions( ICollection<long> versionIds );
  Task BatchActivateVersions( long userId, string userName, ICollection<long> versionIds );
  Task<IEnumerable<object>> GetGroupTasksByVersionId( long versionId, int columnWidth, double amplifiedFactor );
  Task SaveProjectTasks( long versionId, ICollection<CommonGroupTaskFormData> formData );
  Task<ServiceResponse<VersionResource>> DuplicateVersion( long userId, string userName, long projectId, Version oldVersion, string? newVersionName );
  Task<dynamic> GetDataFromFile( long versionId, Stream fileStream, int maxDisplayOrder, string [] sheetNameList );
  Task<dynamic> GetUpdatedDataFromFile( long versionId, Stream fileStream, string [] sheetNameList );
  Task SendVersionsToAnotherProject( long userId, string userName, SendVersionsToAnotherProjectFormData formData );
}
