using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ProjectSettingRepository : GenericRepository<ProjectSetting>, IProjectSettingRepository
{
  public ProjectSettingRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<ProjectSetting?> GetByProjectId( long projectId )
  {
    return ( await _context.ProjectSettings.FromSqlRaw( "CALL usp_ProjectSettings_GetByProjectId({0})", projectId ).ToListAsync() ).FirstOrDefault();
  }
}
