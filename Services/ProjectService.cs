using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Services;

public class ProjectService : IProjectService
{
  private readonly IProjectRepository _projectRepository;
  private readonly IUnitOfWork _unitOfWork;

  public ProjectService( IProjectRepository projectRepository, IUnitOfWork unitOfWork )
  {
    _projectRepository = projectRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IEnumerable<Project>> GetActiveProjects( long userId )
  {
    return await _projectRepository.GetActiveProjects( userId );
  }

  public async Task<Project?> GetProject( long userId, long projectId )
  {
    var project = await _projectRepository.GetById( projectId );
    return ( ( project?.IsActivated ?? false ) && project?.UserId == userId ) ? project : null;
  }

  public async Task<ServiceResponse<Project>> CreateProject( Project project )
  {
    try {
      var newProject = await _projectRepository.Create( project );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Project>( newProject );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Project>( $"{ProjectNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task BatchDeactiveProjects( long userId, ICollection<long> projectIds )
  {
    var projectsToDeactive = await _projectRepository.GetActiveProjects( userId, projectIds );
    if ( !projectsToDeactive.Any() )
      return;

    foreach ( var project in projectsToDeactive ) {
      project.IsActivated = false;
      await _projectRepository.Update( project );
      await _unitOfWork.CompleteAsync();
    }
  }

  public async Task<ServiceResponse<Project>> UpdateProject( Project project )
  {
    try {
      await _projectRepository.Update( project );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Project>( project );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Project>( $"{ProjectNotification.ErrorSaving} {ex.Message}" );
    }
  }
}
