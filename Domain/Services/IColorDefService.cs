using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Services;

public interface IColorDefService
{
  Task<ServiceResponse<ColorDef>> CreateColorDef( ColorDef colorDef );
  Task<ServiceResponse<ColorDef>> DeleteColorDef( long colorDefId );
  Task<ColorDef?> GetColor( long colorId );
  Task<IEnumerable<ColorDef>> GetBackgroundColorDefsByVersionId( long versionId );
  Task<IEnumerable<ColorDef>> GetStepworkColorDefsByVersionId( long versionId );
  Task<ServiceResponse<ColorDef>> UpdateColorDef( ColorDef colorDef );
  Task DuplicateColorDefs( long fromProjectId, long toProjectId );
}