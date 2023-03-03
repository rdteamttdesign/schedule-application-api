using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Services;

namespace SchedulingTool.Api.Controllers;

[Route( "api" )]
[ApiController]
public class ViewsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IViewService _viewService;

  public ViewsController( IMapper mapper, IViewService viewService )
  {
    _mapper = mapper;
    _viewService = viewService;
  }
}
