using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Persistence.Context;

namespace SchedulingTool.Api.Persistence.Repositories;

public class ViewTaskRepository : GenericRepository<ViewTask>
{
  public ViewTaskRepository( AppDbContext context ) : base( context )
  {
  }
}
