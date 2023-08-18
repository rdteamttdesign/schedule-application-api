using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ColorDefRepository : GenericRepository<ColorDef>, IColorDefRepository
{
  public ColorDefRepository( AppDbContext context ) : base( context )
  {

  }

  public async Task<IEnumerable<ColorDef>> GetBackgroundColorsByVersionId( long versionId )
  {
    return await _context.ColorDefs.Where( c => c.VersionId == versionId && c.Type == ( int ) Domain.Models.Enum.ColorType.Background ).ToListAsync();
  }

  public async Task<IEnumerable<ColorDef>> GetStepworkColorsByVersionId( long versionId )
  {
    return await _context.ColorDefs.Where( c => c.VersionId == versionId && c.Type == ( int ) Domain.Models.Enum.ColorType.Stepwork ).ToListAsync();
  }

  public async Task DuplicateColorDefs( long fromVersionId, long toVersionId )
  {
    await _context.Database.ExecuteSqlRawAsync( "CALL usp_Color_DuplicateColor( {0} , {1} )", fromVersionId, toVersionId );
  }
}
