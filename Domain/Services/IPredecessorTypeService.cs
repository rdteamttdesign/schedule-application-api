using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Services;

public interface IPredecessorTypeService
{
  Task<IEnumerable<PredecessorType>> GetAll();
}
