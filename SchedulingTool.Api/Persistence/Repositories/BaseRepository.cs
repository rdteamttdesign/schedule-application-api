using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public abstract class BaseRepository
{
  protected readonly AppDbContext _context;
  public BaseRepository( AppDbContext context )
  {
    _context = context;
  }
}
