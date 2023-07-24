using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Models.Extended;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
  public ProjectRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<List<ProjectVersionDetails>> GetProjectVersionDetails( long userId )
  {
    return await _context.Set<ProjectVersionDetails>().FromSqlRaw( "CALL usp_Project_GetProjectListByProjectId( {0} )", userId ).ToListAsync();
  }
}
