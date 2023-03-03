using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IGroupTaskRepository : IGenericRepository<GroupTask>
{
  Task<IEnumerable<GroupTask>> GetGroupTasksByProjectId( long projectId );
}
