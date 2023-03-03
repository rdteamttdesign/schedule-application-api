using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ColorDefRepository : GenericRepository<ColorDef>, IColorDefRepository
{
  public ColorDefRepository( AppDbContext context ) : base( context )
  {
    
  }

  public async Task<IEnumerable<ColorDef>> GetBackgroundColorDefsByProjectId( long projectId )
  {
    return await _context.ColorDefs.Where( c => c.ProjectId == projectId && c.Type == ( int ) Domain.Models.Enum.ColorType.Background ).ToListAsync();
  }

  public async Task<IEnumerable<ColorDef>> GetStepworkColorDefsByProjectId( long projectId )
  {
    return await _context.ColorDefs.Where( c => c.ProjectId == projectId && c.Type == ( int ) Domain.Models.Enum.ColorType.Stepwork ).ToListAsync();
  }
}
