using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;

namespace SchedulingTool.Api.Services;

public class GroupTaskService : IGroupTaskService
{
  private readonly IGroupTaskRepository _groupTaskRepository;
  private readonly IUnitOfWork _unitOfWork;

  public GroupTaskService( IGroupTaskRepository groupTaskRepository, IUnitOfWork unitOfWork )
  {
    _groupTaskRepository = groupTaskRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<IEnumerable<GroupTask>> GetGroupTasksByVersionId( long versionId )
  {
    return await _groupTaskRepository.GetGroupTasksByVersionId( versionId );
  }

  public async Task<GroupTask?> GetGroupTaskById( long groupTaskId )
  {
    return await _groupTaskRepository.GetById( groupTaskId );
  }

  public async Task<ServiceResponse<GroupTask>> CreateGroupTask( GroupTask groupTask )
  {
    try {
      var newGroupTask = await _groupTaskRepository.Create( groupTask );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<GroupTask>( newGroupTask );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<GroupTask>( $"{GroupTaskNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<GroupTask>> UpdateGroupTask( GroupTask groupTask )
  {
    try {
      await _groupTaskRepository.Update( groupTask );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<GroupTask>( groupTask );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<GroupTask>( $"{GroupTaskNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<GroupTask>> DeleteGroupTask( long groupTaskId )
  {
    try {
      var groupTask = await _groupTaskRepository.GetById( groupTaskId );
      if ( groupTask is null )
        return new ServiceResponse<GroupTask>( GroupTaskNotification.NonExisted );
      _groupTaskRepository.Delete( groupTask );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<GroupTask>( groupTask );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<GroupTask>( $"{GroupTaskNotification.ErrorDeleting} {ex.Message}" );
    }
  }
}
