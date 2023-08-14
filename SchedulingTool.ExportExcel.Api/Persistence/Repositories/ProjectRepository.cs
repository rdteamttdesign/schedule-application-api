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
}