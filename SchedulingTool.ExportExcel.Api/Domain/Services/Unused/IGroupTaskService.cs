using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;

namespace SchedulingTool.Api.Domain.Services.Unused;

public interface IGroupTaskService
{
  Task<IEnumerable<GroupTask>> GetGroupTasksByVersionId( long versionId );
  Task<GroupTask?> GetGroupTaskById( long groupTaskId );
  Task<ServiceResponse<GroupTask>> CreateGroupTask( GroupTask groupTask );
  Task<ServiceResponse<GroupTask>> UpdateGroupTask( GroupTask groupTask );
  Task<ServiceResponse<GroupTask>> DeleteGroupTask( long groupTaskId );
}
