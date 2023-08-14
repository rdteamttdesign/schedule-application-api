using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Resources.Extended;

namespace SchedulingTool.Api.Domain.Services;

public interface IVersionService
{
  Task<ServiceResponse<ExportExcelResource>> ExportToExcel( long versionId );
}
