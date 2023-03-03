using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;

namespace SchedulingTool.Api.Services;

public class BackgroundService : IBackgroundService
{
  private readonly IBackgroundRepository _backgroundRepository;
  private readonly IUnitOfWork _unitOfWork;

  public BackgroundService( IBackgroundRepository backgroundRepository, IUnitOfWork unitOfWork )
  {
    _backgroundRepository = backgroundRepository;
    _unitOfWork = unitOfWork;
  }
}
