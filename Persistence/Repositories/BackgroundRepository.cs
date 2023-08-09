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

  public async Task<ProjectBackground?> GetProjectBackground( long versionId, int month )
  {
    return await _context.ProjectBackgrounds.FindAsync( versionId, month );
  }

  public async Task<IEnumerable<ProjectBackground>> GetBackgroundsByVersionId( long versionId )
  {
    return await _context.ProjectBackgrounds.Where( bg => bg.VersionId == versionId ).ToListAsync();
  }

  public async Task BatchCreate( long versionId, int numberOfMonths )
  {
    await _context.Database.ExecuteSqlRawAsync( "CALL usp_Background_BatchCreate({0} , {1})", versionId, numberOfMonths );
  }

  public async Task BatchDelete( long versionId, int fromMonth )
  {
    await _context.Database.ExecuteSqlRawAsync( "CALL usp_Background_DeleteFromMonth({0} , {1})", versionId, fromMonth );
  }

  public async Task AddMonth( long versionId, int numberOfMonth )
  {
    await _context.Database.ExecuteSqlRawAsync( "CALL usp_Background_AddMonth({0} , {1})", versionId, numberOfMonth );
  }

  public async Task BatchDelete( long versionId )
  {
    await _context.Database.ExecuteSqlRawAsync( "CALL usp_Background_DeleteAll({0})", versionId );
  }
}
