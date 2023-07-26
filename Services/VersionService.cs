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
  private readonly IProjectVersionRepository _projectVersionRepository;
  private readonly IVersionRepository _versionRepository;
  private readonly IUnitOfWork _unitOfWork;

  public VersionService( 
    IProjectVersionRepository projectVersionRepository,
    IVersionRepository versionRepository, 
    IUnitOfWork unitOfWork )
  {
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
    var versionsToDeactive = await _versionRepository.GetActiveVersions( userId, versionIds );
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
}
