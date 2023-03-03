using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IProjectSettingRepository : IGenericRepository<ProjectSetting>
{
  Task<ProjectSetting?> GetByProjectId( long projectId );
}
