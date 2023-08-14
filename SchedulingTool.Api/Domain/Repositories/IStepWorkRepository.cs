using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IStepworkRepository : IGenericRepository<Stepwork>
{
  Task<IEnumerable<Stepwork>> GetStepworksByTaskId( long taskId );
  Task<IEnumerable<Stepwork>> GetStepworksByTaskLocalId( string taskLocalId );
}
