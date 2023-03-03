using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;

namespace SchedulingTool.Api.Services;

public class ViewService : IViewService
{
  private readonly IViewRepository _viewRepository;
  private readonly IUnitOfWork _unitOfWork;

  public ViewService( IViewRepository viewRepository, IUnitOfWork unitOfWork )
  {
    _viewRepository = viewRepository;
    _unitOfWork = unitOfWork;
  }
}
