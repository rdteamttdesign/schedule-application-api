﻿using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;
using Task = System.Threading.Tasks.Task;
using Version = SchedulingTool.Api.Domain.Models.Version;

namespace SchedulingTool.Api.Services;

public class VersionService : IVersionService
{
  private readonly IProjectRepository _projectRepository;
  private readonly IProjectVersionRepository _projectVersionRepository;
  private readonly IVersionRepository _versionRepository;
  private readonly IUnitOfWork _unitOfWork;

  public VersionService(
    IProjectRepository projectRepository,
    IProjectVersionRepository projectVersionRepository,
    IVersionRepository versionRepository,
    IUnitOfWork unitOfWork )
  {
    _projectRepository = projectRepository;
    _projectVersionRepository = projectVersionRepository;
    _versionRepository = versionRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IEnumerable<Version>> GetActiveVersions( long userId, long projectId )
  {
    var versions = await _versionRepository.GetActiveVersions( userId );
    var projectVersions = await _projectVersionRepository.GetByProjectId( projectId );
    var versionIdList = projectVersions.Select( v => v.VersionId );
    return versions.Where( x => versionIdList.Contains( x.VersionId ) );
  }

  public async Task BatchDeleteVersionDetails( long versionId )
  {
    await _versionRepository.BatchDeleteVersionDetails( versionId );
  }

  public async Task<Version?> GetVersionById( long versionId )
  {
    var version = await _versionRepository.GetById( versionId );
    return ( version?.IsActivated ?? false ) ? version : null;
  }

  public async Task<ServiceResponse<Version>> CreateVersion( long projectId, Version version )
  {
    try {
      var newVersion = await _versionRepository.Create( version );
      await _unitOfWork.CompleteAsync();
      await _projectVersionRepository.Create( new ProjectVersion() { ProjectId = projectId, VersionId = version.VersionId } );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Version>( newVersion );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Version>( $"{ProjectNotification.ErrorSaving} {ex.Message}. {ex.InnerException?.Message}" );
    }
  }

  public async Task BatchDeactiveVersions( long userId, ICollection<long> versionIds )
  {
    var versionsToDeactive = await _versionRepository.GetVersionsById( userId, versionIds );
    if ( !versionsToDeactive.Any() )
      return;

    foreach ( var version in versionsToDeactive ) {
      version.IsActivated = false;
      await _versionRepository.Update( version );
      await _unitOfWork.CompleteAsync();
    }
  }

  public async Task<ServiceResponse<Version>> UpdateVersion( Version version )
  {
    try {
      await _versionRepository.Update( version );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Version>( version );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Version>( $"{ProjectNotification.ErrorSaving} {ex.Message}. {ex.InnerException?.Message}" );
    }
  }

  public async Task BatchDeleteVersions( ICollection<long> versionIds )
  {
    var projectIdList = ( await _projectVersionRepository.GetAll() ).Where( x => versionIds.Contains( x.VersionId ) ).Select( x => x.ProjectId ).Distinct();
    foreach ( var versionId in versionIds ) {
      await _versionRepository.BatchDeleteVersion( versionId );
    }
  }

  public async Task BatchActivateVersions( long userId, ICollection<long> versionIds )
  {
    var activatedVersionIds = ( await _versionRepository.GetActiveVersions( userId ) ).Select( x => x.VersionId );
    var projectVersions = await _projectVersionRepository.GetAll();
    var activatedProjectVersions = projectVersions.Where( x => activatedVersionIds.Contains( x.VersionId ) );
    var projectIdList = projectVersions.Where( x => versionIds.Contains( x.VersionId ) ).Select( x => x.ProjectId );
    var projectIdsToActivate = projectIdList.Where( x => !activatedProjectVersions.Any( pv => pv.ProjectId == x ) ).Distinct();

    foreach ( var projectId in projectIdsToActivate ) {
      var project = await _projectRepository.GetById( projectId );
      project.ProjectName += $" (restored at {( int ) DateTime.UtcNow.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds})";
      project.ModifiedDate = DateTime.UtcNow;
      await _projectRepository.Update( project );
    }

    var versionsToActivate = await _versionRepository.GetVersionsById( userId, versionIds );
    if ( !versionsToActivate.Any() )
      return;

    foreach ( var version in versionsToActivate ) {
      version.VersionName += $" (restored at {( int ) DateTime.UtcNow.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds})";
      version.ModifiedDate = DateTime.UtcNow;
      version.IsActivated = true;
      await _versionRepository.Update( version );
    }
    await _unitOfWork.CompleteAsync();
  }
}
