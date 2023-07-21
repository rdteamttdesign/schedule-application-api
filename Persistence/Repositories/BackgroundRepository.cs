using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Persistence.Repositories;

public class BackgroundRepository : GenericRepository<ProjectBackground>, IBackgroundRepository
{
  public BackgroundRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<ProjectBackground?> GetProjectBackground( long projectId, int month )
  {
    return await _context.ProjectBackgrounds.FindAsync( projectId, month );
  }

  public async Task<IEnumerable<ProjectBackground>> GetBackgroundsByProjectId( long projectId )
  {
    return await _context.ProjectBackgrounds.Where( bg => bg.VersionId == projectId ).ToListAsync();
  }

  public async Task BatchCreate( long projectId, int numberOfMonths )
  {
    await _context.Database.ExecuteSqlRawAsync( "CALL usp_Background_BatchCreate({0} , {1})", projectId, numberOfMonths );
  }

  public async Task BatchDelete( long projectId, int fromMonth )
  {
    await _context.Database.ExecuteSqlRawAsync( "CALL usp_Background_DeleteFromMonth({0} , {1})", projectId, fromMonth );
  }

  public async Task AddMonth( long projectId, int numberOfMonth )
  {
    await _context.Database.ExecuteSqlRawAsync( "CALL usp_Background_AddMonth({0} , {1})", projectId, numberOfMonth );
  }
}
