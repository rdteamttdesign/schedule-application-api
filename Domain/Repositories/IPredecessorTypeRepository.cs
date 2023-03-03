using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IPredecessorTypeRepository : IGenericRepository<PredecessorType>
{
  Task<IEnumerable<PredecessorType>> GetAll();
}
