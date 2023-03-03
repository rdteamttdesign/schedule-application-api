using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;

namespace SchedulingTool.Api.Services;

public class PredecessorTypeService : IPredecessorTypeService
{
  private readonly IPredecessorTypeRepository _predecessorTypeRepository;

  public PredecessorTypeService( IPredecessorTypeRepository predecessorTypeRepository )
  {
    _predecessorTypeRepository = predecessorTypeRepository;
  }

  public async Task<IEnumerable<PredecessorType>> GetAll()
  {
    return await _predecessorTypeRepository.GetAll();
  }
}
