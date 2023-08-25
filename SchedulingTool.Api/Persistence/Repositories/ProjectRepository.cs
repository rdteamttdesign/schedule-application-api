using DocumentFormat.OpenXml.Spreadsheet;
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
    return await _context.Set<ProjectVersionDetails>().FromSqlRaw( "CALL usp_Project_GetProjectListByUserId( {0} )", userId ).ToListAsync();
  }

  public async Task<IEnumerable<Project>> GetMyProjects( long userId )
  {
    return await _context.Projects.Where( project => project.UserId == userId ).ToListAsync();
  }

  public async Task<List<ProjectVersionDetails>> GetSharedProjectVersionDetails()
  {
    return await _context.Set<ProjectVersionDetails>().FromSqlRaw( "CALL usp_Project_GetSharedProjectList()" ).ToListAsync();
  }

  public async Task<IEnumerable<Project>> GetAllProjects( long userId )
  {
    return await _context.Projects.Where( project => project.UserId == userId || project.IsShared ).ToListAsync();
  }
}
