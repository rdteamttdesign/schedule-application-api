using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
  public ProjectRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<IEnumerable<Project>> GetActiveProjects( long userId )
  {
    return await _context.Projects.Where( project => project.UserId == userId && project.IsActivated ).ToListAsync();
  }

  public async Task<IEnumerable<Project>> GetActiveProjects( long userId, ICollection<long> projectIds )
  {
    return await _context.Projects.Where( project => project.UserId == userId && projectIds.Contains( project.ProjectId ) ).ToListAsync();
  }

  public async Task BatchDeleteProjectDetails( long projectId )
  {
    await _context.Database.ExecuteSqlRawAsync( "CALL usp_Project_BatchDeleteDetails( {0} )", projectId );
  }
}
