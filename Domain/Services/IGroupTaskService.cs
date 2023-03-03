using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;

namespace SchedulingTool.Api.Domain.Services;

public interface IGroupTaskService
{
  Task<IEnumerable<GroupTask>> GetGroupTasksByProjectId( long projectId );
  Task<GroupTask?> GetGroupTaskById( long groupTaskId );
  Task<ServiceResponse<GroupTask>> CreateGroupTask( GroupTask groupTask );
  Task<ServiceResponse<GroupTask>> UpdateGroupTask( GroupTask groupTask );
  Task<ServiceResponse<GroupTask>> DeleteGroupTask( long groupTaskId );
}
