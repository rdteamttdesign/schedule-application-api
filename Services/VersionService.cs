﻿using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;
using Task = System.Threading.Tasks.Task;
using Version = SchedulingTool.Api.Domain.Models.Version;

namespace SchedulingTool.Api.Services;

public class VersionService : IVersionService
{
  private readonly IVersionRepository _versionRepository;
  private readonly IUnitOfWork _unitOfWork;

  public VersionService( IVersionRepository versionRepository, IUnitOfWork unitOfWork )
  {
    _versionRepository = versionRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IEnumerable<Version>> GetActiveVersions( long userId)
  {
    return await _versionRepository.GetActiveVersions( userId );
  }

  public async Task BatchDeleteVersionDetails( long versionId )
  {
    await _versionRepository.BatchDeleteVersionDetails( versionId );
  }

  public async Task<Version?> GetVersion( long userId, long versionId )
  {
    var version = await _versionRepository.GetById( versionId );
    return ( ( version?.IsActivated ?? false ) && version?.UserId == userId ) ? version : null;
  }

  public async Task<ServiceResponse<Version>> CreateVersion( Version version )
  {
    try {
      var newVersion = await _versionRepository.Create( version );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<Version>( newVersion );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<Version>( $"{ProjectNotification.ErrorSaving} {ex.Message}" );
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
      return new ServiceResponse<Version>( $"{ProjectNotification.ErrorSaving} {ex.Message}" );
    }
  }
}