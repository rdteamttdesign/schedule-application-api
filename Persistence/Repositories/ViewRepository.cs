using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ViewRepository : GenericRepository<View>, IViewRepository
{
  public ViewRepository( AppDbContext context ) : base( context )
  {
  }
}
