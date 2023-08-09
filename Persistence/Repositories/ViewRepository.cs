using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;
using SchedulingTool.Api.Resources;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ViewRepository : GenericRepository<View>, IViewRepository
{
  public ViewRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<IEnumerable<View>> GetViewsByVersionId( long versionId )
  {
    return await _context.Views.Where( view => view.VersionId == versionId ).ToListAsync();
  }

  public async Task<IEnumerable<ViewTaskDetail>> GetViewTasks( long versionId, long viewId )
  {
    return await _context.Set<ViewTaskDetail>().FromSqlRaw( "CALL usp_View_GetTasks( {0}, {1} )", versionId, viewId ).ToListAsync();
  }
}
