using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Resources;
using Version = SchedulingTool.Api.Domain.Models.Version;

namespace SchedulingTool.Api.Domain.Services;

public interface IProjectService
{
  Task<Project?> GetProjectById( long projectId );
  Task<ServiceResponse<Project>> UpdateProject( Project project );
  Task<IEnumerable<ProjectListResource>> GetProjectListByUserId( long userId, bool isActivated );
  Task<ServiceResponse<ProjectListResource>> CreateProject( Project project, Version defaultVersion );
  Task<IEnumerable<Project>> GetActiveProjects( long userId );
  //Task<IEnumerable<ProjectListResource>> GetDeactiveProjectListByUserId( long userId );
  Task<IEnumerable<object>> GetSharedActiveProjectNameList();
  Task<IEnumerable<ProjectListResource>> GetSharedProjectList( bool isActivated );
}
