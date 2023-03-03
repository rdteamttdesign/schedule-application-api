using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Services;

namespace SchedulingTool.Api.Controllers;

[Route( "api/[controller]" )]
[ApiController]
public class ProjectBackgroundsController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IBackgroundService _backgroundService;

  public ProjectBackgroundsController( IMapper mapper, IBackgroundService backgroundService )
  {
    _mapper = mapper;
    _backgroundService = backgroundService;
  }
}
