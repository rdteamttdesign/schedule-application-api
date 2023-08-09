using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services.Unused;

namespace SchedulingTool.Api.Services.Unused;

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
