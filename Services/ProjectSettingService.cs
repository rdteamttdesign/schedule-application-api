using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;

namespace SchedulingTool.Api.Services;

public class ProjectSettingService : IProjectSettingService
{
  private readonly IProjectSettingRepository _projectSettingRepository;
  private readonly IUnitOfWork _unitOfWork;

  public ProjectSettingService( IUnitOfWork unitOfWork, IProjectSettingRepository projectSettingRepository )
  {
    _unitOfWork = unitOfWork;
    _projectSettingRepository = projectSettingRepository;
  }

  public async Task<ProjectSetting?> GetProjectSetting( long projectId )
  {
    return await _projectSettingRepository.GetByProjectId( projectId );
  }

  public async Task<ServiceResponse<ProjectSetting>> CreateProjectSetting( ProjectSetting setting )
  {
    try {
      var newSetting = await _projectSettingRepository.Create( setting );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ProjectSetting>( newSetting );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ProjectSetting>( $"{ProjectSettingNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<ProjectSetting>> UpdateProjectSetting( ProjectSetting setting )
  {
    try {
      await _projectSettingRepository.Update( setting );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ProjectSetting>( setting );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ProjectSetting>( $"{ProjectSettingNotification.ErrorSaving} {ex.Message}" );
    }
  }
}
