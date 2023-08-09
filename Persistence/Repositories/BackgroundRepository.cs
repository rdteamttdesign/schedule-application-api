using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public class BackgroundRepository : GenericRepository<ProjectBackground>, IBackgroundRepository
{
  public BackgroundRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<IEnumerable<ProjectBackground>> GetBackgroundsByVersionId( long versionId )
  {
    return await _context.ProjectBackgrounds.Where( bg => bg.VersionId == versionId ).ToListAsync();
  }
}
