using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ProjectVersionRepository : GenericRepository<ProjectVersion>, IProjectVersionRepository
{
  public ProjectVersionRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<ProjectVersion?> GetProjectVersionByVersionId( long versionId )
  {
    return await _context.ProjectVersions.FirstOrDefaultAsync( x => x.VersionId == versionId );
  }
}
