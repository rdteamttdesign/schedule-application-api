using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Domain.Services;

public interface IViewService
{
  Task<IEnumerable<View>> GetViewsByVersionId( long versionId );
  Task<ServiceResponse<ViewResource>> CreateView( long versionId, ViewFormData formData );
  Task<ServiceResponse<ViewResource>> UpdateView( long versionId, long viewId, ViewFormData formData );
  Task<ServiceResponse<IEnumerable<object>>> GetViewDetailById( long versionId, long viewId );
  Task DeleteView( long viewId, bool isDeleteView );
  Task<ServiceResponse<View>> SaveViewDetail( long viewId, ICollection<ViewTaskDetailFormData> formData );
  Task DuplicateView( long fromVersionId, long toVersionId );
}
