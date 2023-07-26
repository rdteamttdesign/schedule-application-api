using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ProjectVersionRepository : GenericRepository<ProjectVersion>, IProjectVersionRepository
{
  public ProjectVersionRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<IEnumerable<ProjectVersion>> GetByProjectId( long projectId )
  {
    return await _context.ProjectVersions.Where( x => x.ProjectId == projectId ).ToListAsync();
  }
}
