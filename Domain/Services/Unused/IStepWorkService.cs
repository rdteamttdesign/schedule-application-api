using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;

namespace SchedulingTool.Api.Domain.Services.Unused;

public interface IStepworkService
{
  Task<IEnumerable<Stepwork>> GetStepworksByTaskId( long taskId );
  Task<Stepwork?> GetStepworkById( long stepworkId );
  Task<ServiceResponse<Stepwork>> CreateStepwork( Stepwork stepwork );
  Task<ServiceResponse<Stepwork>> UpdateStepwork( Stepwork stepwork );
  Task<ServiceResponse<Stepwork>> DeleteStepwork( long stepworkId );
}
