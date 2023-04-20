using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Services;

public interface IColorDefService
{
  Task<ServiceResponse<ColorDef>> CreateColorDef( ColorDef colorDef );
  Task<ServiceResponse<ColorDef>> DeleteColorDef( long colorDefId );
  Task<ColorDef?> GetColor( long colorId );
  Task<IEnumerable<ColorDef>> GetBackgroundColorDefsByProjectId( long projectId );
  Task<IEnumerable<ColorDef>> GetStepworkColorDefsByProjectId( long projectId );
  Task<ServiceResponse<ColorDef>> UpdateColorDef( ColorDef colorDef );
  Task DuplicateColorDefs( long fromProjectId, long toProjectId );
}