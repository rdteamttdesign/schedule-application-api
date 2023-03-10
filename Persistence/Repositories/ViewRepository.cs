﻿using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ViewRepository : GenericRepository<View>, IViewRepository
{
  public ViewRepository( AppDbContext context ) : base( context )
  {
  }

  public async Task<IEnumerable<View>> GetViewsByProjectId(long projectId)
  {
    return await _context.Views.Where( view => view.ProjectId == projectId ).ToListAsync();
  }

  public async Task DeleteView( long viewId )
  {
    await _context.Database.ExecuteSqlRawAsync( "", viewId );
  }
}
