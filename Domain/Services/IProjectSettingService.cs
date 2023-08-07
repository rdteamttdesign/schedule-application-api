using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;

namespace SchedulingTool.Api.Domain.Services;

public interface IProjectSettingService
{
  Task<ProjectSetting?> GetProjectSetting( long versionId );

  Task<ServiceResponse<ProjectSetting>> CreateProjectSetting( ProjectSetting setting );

  Task<ServiceResponse<ProjectSetting>> UpdateProjectSetting( ProjectSetting setting );
}
