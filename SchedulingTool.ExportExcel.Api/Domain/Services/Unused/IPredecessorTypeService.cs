using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.Domain.Services.Unused;

public interface IPredecessorTypeService
{
  Task<IEnumerable<PredecessorType>> GetAll();
}
