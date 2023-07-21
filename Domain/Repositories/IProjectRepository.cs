using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Models.Extended;

namespace SchedulingTool.Api.Domain.Repositories;

public interface IProjectRepository : IGenericRepository<Project>
{
  Task<List<ProjectVersionDetails>> GetProjectVersionDetails( long userId );
}
