using AutoMapper;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using Version = SchedulingTool.Api.Domain.Models.Version;

namespace SchedulingTool.Api.Services;

public class ProjectService : IProjectService
{
  private readonly IProjectRepository _projectRepository;
  private readonly IProjectVersionRepository _projectVersionRepository;
  private readonly IVersionRepository _versionRepository;
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;

  public ProjectService(
    IVersionRepository versionRepository,
    IProjectRepository projectRepository,
    IProjectVersionRepository projectVersionRepository,
    IMapper mapper,
    IUnitOfWork unitOfWork )
  {
    _projectRepository = projectRepository;
    _projectVersionRepository = projectVersionRepository;
    _versionRepository = versionRepository;
    _mapper = mapper;
    _unitOfWork = unitOfWork;
  }

  public async Task<ServiceResponse<ProjectListResource>> CreateProject( Project project, Version defaultVersion )
  {
    try {
      var newProject = await _projectRepository.Create( project );
      await _unitOfWork.CompleteAsync();
      var newVersion = await _versionRepository.Create( defaultVersion );
      await _unitOfWork.CompleteAsync();
      await _projectVersionRepository.Create( new ProjectVersion() { ProjectId = newProject.ProjectId, VersionId = newVersion.VersionId } );
      await _unitOfWork.CompleteAsync();
      var result = new ProjectListResource()
      {
        ProjectName = newProject.ProjectName,
        ProjectId = newProject.ProjectId,
        ModifiedDate = newVersion.ModifiedDate
      };
      result.Versions.Add( _mapper.Map<VersionResource>( newVersion ) );
      return new ServiceResponse<ProjectListResource>( result );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ProjectListResource>( $"{ex.Message}: {ex.StackTrace}" );
    }
  }

  public async Task<Project?> GetProjectById( long projectId )
  {
    return await _projectRepository.GetById( projectId );
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

  public async Task<IEnumerable<ProjectListResource>> GetProjectListByUserId( long userId )
  {
    var projectVersions = await _projectRepository.GetProjectVersionDetails( userId );
    var groupByProject = projectVersions.Where( version => version.IsActivated ).GroupBy( x => new { ProjectId = x.ProjectId, ProjectName = x.ProjectName } );
    var projectResources = new List<ProjectListResource>();
    foreach ( var group in groupByProject ) {
      var projectResource = new ProjectListResource()
      {
        ProjectId = group.Key.ProjectId,
        ProjectName = group.Key.ProjectName
      };
      foreach ( var version in group.OrderBy( x => x.ModifiedDate ) ) {
        var versionResource = _mapper.Map<VersionResource>( version );
        projectResource.Versions.Add( versionResource );
      }
      projectResources.Add( projectResource );
    }
    return projectResources;
  }
}
