using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;

namespace SchedulingTool.Api.Domain.Services;

public interface IProjectSettingService
{
  Task<ProjectSetting?> GetProjectSetting( long versionId );

  Task<ServiceResponse<ProjectSettingResource>> GetProjectSettingByVersionId( long versionId );

  Task<ServiceResponse<ProjectSetting>> CreateProjectSetting( ProjectSetting setting );

  Task<ServiceResponse<ProjectSetting>> UpdateProjectSetting( ProjectSetting setting );

  Task<ServiceResponse<ProjectSetting>> UpdateProjectSettingByVersionId( long versionId, ProjectSettingFormData formData );
}
