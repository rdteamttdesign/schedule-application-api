using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;
using Task = System.Threading.Tasks.Task;
using Version = SchedulingTool.Api.Domain.Models.Version;

namespace SchedulingTool.Api.Persistence.Repositories;

public class VersionRepository : GenericRepository<Version>, IVersionRepository
{
  public VersionRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<IEnumerable<Version>> GetActiveVersions( long userId )
  {
    return await _context.Versions.Where( version => version.UserId == userId && version.IsActivated ).ToListAsync();
  }

  public async Task<IEnumerable<Version>> GetActiveVersions( long userId, ICollection<long> versionIds )
  {
    return await _context.Versions.Where( version => version.UserId == userId && versionIds.Contains( version.VersionId ) ).ToListAsync();
  }

  public async Task BatchDeleteVersionDetails( long versionId )
  {
    await _context.Database.ExecuteSqlRawAsync( "CALL usp_Project_BatchDeleteDetails( {0} )", versionId );
  }
}
