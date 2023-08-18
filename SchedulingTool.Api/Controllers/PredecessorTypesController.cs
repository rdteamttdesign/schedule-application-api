using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulingTool.Api.Domain.Services;

namespace SchedulingTool.Api.Controllers;

[Route( "api/[controller]" )]
[ApiController]
public class PredecessorTypesController : ControllerBase
{
  private readonly IMapper _mapper;
  private readonly IPredecessorTypeService _predecessorTypeService;

  public PredecessorTypesController( IMapper mapper, IPredecessorTypeService predecessorTypeService )
  {
    _mapper = mapper;
    _predecessorTypeService = predecessorTypeService;
  }

  [HttpGet()]
  [Authorize]
  public async Task<IActionResult> GetPredecessorTypes()
  {
    var predecessorTypes = await _predecessorTypeService.GetAll();
    return Ok( predecessorTypes );
  }
}
