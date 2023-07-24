using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Notification;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Services;

public class ColorDefService : IColorDefService
{
  private readonly IColorDefRepository _colorDefRepository;
  private readonly IUnitOfWork _unitOfWork;

  public ColorDefService( IColorDefRepository colorDefRepository, IUnitOfWork unitOfWork )
  {
    _colorDefRepository = colorDefRepository;
    _unitOfWork = unitOfWork;
  }

  public async Task<ColorDef?> GetColor( long colorId )
  {
    return await _colorDefRepository.GetById( colorId );
  }

  public async Task<IEnumerable<ColorDef>> GetBackgroundColorDefsByVersionId( long projectId )
  {
    return await _colorDefRepository.GetBackgroundColorDefsByProjectId( projectId );
  }

  public async Task<IEnumerable<ColorDef>> GetStepworkColorDefsByVersionId( long projectId )
  {
    return await _colorDefRepository.GetStepworkColorDefsByProjectId( projectId );
  }

  public async Task<ServiceResponse<ColorDef>> CreateColorDef( ColorDef colorDef )
  {
    try {
      var newColor = await _colorDefRepository.Create( colorDef );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ColorDef>( newColor );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ColorDef>( $"{ColorDefNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<ColorDef>> UpdateColorDef( ColorDef colorDef )
  {
    try {
      await _colorDefRepository.Update( colorDef );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ColorDef>( colorDef );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ColorDef>( $"{ColorDefNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<ColorDef>> DeleteColorDef( long colorDefId )
  {
    try {
      var colorDef = await _colorDefRepository.GetById( colorDefId );
      if ( colorDef is null )
        return new ServiceResponse<ColorDef>( ColorDefNotification.NonExisted );
      _colorDefRepository.Delete( colorDef );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ColorDef>( colorDef );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ColorDef>( $"{ColorDefNotification.ErrorDeleting} {ex.Message}" );
    }
  }

  public async Task DuplicateColorDefs( long fromProjectId, long toProjectId )
  {
    await _colorDefRepository.DuplicateColorDefs( fromProjectId, toProjectId );
  }

  public async Task<IEnumerable<ColorDef>> GetAllColorDefsByProjectId( long projectId )
  {
    return await _colorDefRepository.GeAllColorDefsByProjectId( projectId );
  }
}
