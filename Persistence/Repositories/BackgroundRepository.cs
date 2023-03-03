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

  public async Task<ProjectBackground?> GetBackground( long projectId, int month )
  {
    return await _context.ProjectBackgrounds.FindAsync( projectId, month );
  }

  public async Task<IEnumerable<ProjectBackground>> GetBackgroundsByProjectId( long projectId )
  {
    return await _context.ProjectBackgrounds.Where( bg => bg.ProjectId == projectId ).ToListAsync();
  }

  public async Task Create( long projectId, int numberOfMonths )
  {
    await _context.Database.ExecuteSqlRawAsync( "CALL usp_Background_BatchCreate({0} , {1})", projectId, numberOfMonths );
  }
}
