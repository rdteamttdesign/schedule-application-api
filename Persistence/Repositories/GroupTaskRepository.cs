using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public class GroupTaskRepository : GenericRepository<GroupTask>, IGroupTaskRepository
{
  public GroupTaskRepository( AppDbContext context ) : base( context )
  {
  }
  
  public async Task<IEnumerable<GroupTask>> GetGroupTasksByProjectId( long projectId )
  {
    return await _context.GroupTasks.Where( gt => gt.VersionId == projectId ).ToListAsync();
  }
}
