using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ViewRepository : GenericRepository<View>, IViewRepository
{
  public ViewRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<IEnumerable<View>> GetViewsByProjectId( long projectId )
  {
    return await _context.Views.Where( view => view.VersionId == projectId ).ToListAsync();
  }

  public async Task DeleteView( long viewId, bool isDeleteView )
  {
    await _context.Database.ExecuteSqlRawAsync( "CALL usp_View_DeleteViewById( {0}, {1} )", viewId, isDeleteView ? 1 : 0 );
  }

  public async Task<IEnumerable<ViewTaskDetail>> GetViewTasks( long projectId, long viewId )
  {
    return await _context.Set<ViewTaskDetail>().FromSqlRaw( "CALL usp_View_GetTasks( {0}, {1} )", projectId, viewId ).ToListAsync();
  }
}
