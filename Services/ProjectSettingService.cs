using AutoMapper;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Domain.Services.Communication;
using SchedulingTool.Api.Extension;
using SchedulingTool.Api.Notification;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.FormBody;
using DateTimeExt = SchedulingTool.Api.Extension.DateRangeDisplayExtension.DateTime;
using Task = System.Threading.Tasks.Task;

namespace SchedulingTool.Api.Services;

public class ProjectSettingService : IProjectSettingService
{
  private readonly IProjectSettingRepository _projectSettingRepository;
  private readonly IVersionRepository _versionRepository;
  private readonly IColorDefRepository _colorRepository;
  private readonly IBackgroundRepository _backgroundRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IMapper _mapper;

  public ProjectSettingService(
    IUnitOfWork unitOfWork,
    IProjectSettingRepository projectSettingRepository,
    IVersionRepository versionRepository,
    IColorDefRepository colorRepository,
    IBackgroundRepository backgroundRepository,
    IMapper mapper )
  {
    _unitOfWork = unitOfWork;
    _projectSettingRepository = projectSettingRepository;
    _mapper = mapper;
    _versionRepository = versionRepository;
    _colorRepository = colorRepository;
    _backgroundRepository = backgroundRepository;
  }

  public async Task<ProjectSetting?> GetProjectSetting( long projectId )
  {
    return await _projectSettingRepository.GetByVersionId( projectId );
  }

  public async Task<ServiceResponse<ProjectSetting>> CreateProjectSetting( ProjectSetting setting )
  {
    try {
      var newSetting = await _projectSettingRepository.Create( setting );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ProjectSetting>( newSetting );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ProjectSetting>( $"{ProjectSettingNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<ProjectSetting>> UpdateProjectSetting( ProjectSetting setting )
  {
    try {
      await _projectSettingRepository.Update( setting );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ProjectSetting>( setting );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ProjectSetting>( $"{ProjectSettingNotification.ErrorSaving} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<ProjectSettingResource>> GetProjectSettingByVersionId( long versionId )
  {
    var version = await _versionRepository.GetById( versionId );
    if ( version == null ) {
      return new ServiceResponse<ProjectSettingResource>( ProjectSettingNotification.NonExisted );
    }
    var setting = await _projectSettingRepository.GetByVersionId( versionId );
    var resource = _mapper.Map<ProjectSettingResource>( setting );
    resource.NumberOfMonths = version!.NumberOfMonths;

    var stepworkColors = await _colorRepository.GetStepworkColorsByVersionId( versionId );
    resource.StepworkColors = _mapper.Map<IEnumerable<ColorDefResource>>( stepworkColors ).ToList();

    var backgroundColors = await _colorRepository.GetBackgroundColorsByVersionId( versionId );
    resource.BackgroundColors = _mapper.Map<IEnumerable<BackgroundColorResource>>( backgroundColors ).ToList();

    var backgrounds = await _backgroundRepository.GetBackgroundsByVersionId( versionId );
    var backgroundsGroupBy = backgrounds.Where( bg => bg.ColorId != null ).GroupBy( bg => bg.ColorId )?.ToDictionary( g => g.Key, g => g );
    foreach ( var backgroundColor in resource.BackgroundColors ) {
      if ( !backgroundsGroupBy!.ContainsKey( backgroundColor.ColorId ) ) {
        continue;
      }
      var dates = backgroundsGroupBy [ backgroundColor.ColorId ].Select( bg => new DateTimeExt( bg.Year, bg.Month, bg.Date ) );
      foreach ( var diaplayDateRangeText in dates.ToFormatString() ) {
        var fromDateText = diaplayDateRangeText.Split( "-" ) [ 0 ].Trim().Split( "/" );
        var toDateText = diaplayDateRangeText.Split( "-" ) [ 1 ].Trim().Split( "/" );
        if ( setting!.IncludeYear ) {
          backgroundColor.DateRanges.Add( new BackgroundDateRange()
          {
            DisplayText = diaplayDateRangeText,
            StartYear = int.Parse( fromDateText [ 0 ] ),
            StartMonth = int.Parse( fromDateText [ 1 ] ),
            StartDate = int.Parse( fromDateText [ 2 ] ),
            ToYear = int.Parse( toDateText [ 0 ] ),
            ToMonth = int.Parse( toDateText [ 1 ] ),
            ToDate = int.Parse( toDateText [ 2 ] )
          } );
        }
        else {
          backgroundColor.DateRanges.Add( new BackgroundDateRange()
          {
            DisplayText = diaplayDateRangeText,
            StartYear = -1,
            StartMonth = int.Parse( fromDateText [ 0 ] ),
            StartDate = int.Parse( fromDateText [ 1 ] ),
            ToYear = -1,
            ToMonth = int.Parse( toDateText [ 0 ] ),
            ToDate = int.Parse( toDateText [ 1 ] )
          } );
        }
      }
    }
    return new ServiceResponse<ProjectSettingResource>( resource );
  }

  public async Task<ServiceResponse<ProjectSetting>> UpdateProjectSettingByVersionId( long versionId, ProjectSettingFormData formData )
  {
    var version = await _versionRepository.GetById( versionId );
    if ( version == null ) {
      return new ServiceResponse<ProjectSetting>( ProjectSettingNotification.NonExisted );
    }

    await UpdateBackgroundSetting( versionId, formData );

    version.NumberOfMonths = formData.NumberOfMonths;
    await _versionRepository.Update( version );
    await _unitOfWork.CompleteAsync();

    var setting = await _projectSettingRepository.GetByVersionId( versionId );
    if ( setting is null )
      return new ServiceResponse<ProjectSetting>( ProjectSettingNotification.NonExisted );
    setting.SeparateGroupTask = formData.SeparateGroupTask;
    setting.AssemblyDurationRatio = formData.AssemblyDurationRatio;
    setting.RemovalDurationRatio = formData.RemovalDurationRatio;
    setting.ColumnWidth = formData.ColumnWidth;
    setting.AmplifiedFactor = formData.AmplifiedFactor;
    setting.IncludeYear = formData.IncludeYear;
    setting.StartYear = formData.StartYear;
    setting.StartMonth = formData.StartMonth;
    var result = await UpdateProjectSetting( setting );
    if ( !result.Success )
      return new ServiceResponse<ProjectSetting>( result.Message );

    await UpdateStepworkColors( versionId, formData );

    return new ServiceResponse<ProjectSetting>( result.Content );
  }

  private async Task UpdateStepworkColors( long versionId, ProjectSettingFormData formData )
  {
    var existingStepworkColors = await _colorRepository.GetStepworkColorsByVersionId( versionId );
    foreach ( var newStepworkColorData in formData.StepworkColors ) {
      if ( newStepworkColorData.ColorId == null ) {
        var newColor = new ColorDef()
        {
          Name = newStepworkColorData.Name,
          Code = newStepworkColorData.Code,
          Type = 2,
          VersionId = versionId
        };
        await CreateColorDef( newColor );
      }
      else {
        var existingColor = existingStepworkColors.FirstOrDefault( color => color.ColorId == newStepworkColorData.ColorId );
        if ( existingColor == null ) {
          continue;
        }
        if ( !existingColor.IsDefault )
          existingColor.Name = newStepworkColorData.Name;
        existingColor.Code = newStepworkColorData.Code;
        await UpdateColorDef( existingColor );
      }
    }

    var deletingStepworkColors = existingStepworkColors.Where( color => !formData.StepworkColors.Any( x => x.ColorId == color.ColorId ) );
    foreach ( var color in deletingStepworkColors ) {
      await DeleteColorDef( color.ColorId );
    }
  }

  private async Task UpdateBackgroundSetting( long versionId, ProjectSettingFormData formData )
  {
    await _backgroundRepository.BatchDelete( versionId );
    var newBackgrounds = new List<ProjectBackground>();
    if ( formData.IncludeYear ) {
      var year = formData.StartYear;
      var month = formData.StartMonth;
      for ( int i = 0; i < formData.NumberOfMonths; i++ ) {
        newBackgrounds.Add( new ProjectBackground() { VersionId = versionId, Year = year, Month = month, Date = 10 } );
        newBackgrounds.Add( new ProjectBackground() { VersionId = versionId, Year = year, Month = month, Date = 20 } );
        newBackgrounds.Add( new ProjectBackground() { VersionId = versionId, Year = year, Month = month, Date = 30 } );
        if ( month == 12 ) {
          month = 1;
          year++;
        }
        else {
          month++;
        }
      }
    }
    else {
      for ( int i = 0; i < formData.NumberOfMonths; i++ ) {
        newBackgrounds.Add( new ProjectBackground() { VersionId = versionId, Year = -1, Month = i, Date = 10 } );
        newBackgrounds.Add( new ProjectBackground() { VersionId = versionId, Year = -1, Month = i, Date = 20 } );
        newBackgrounds.Add( new ProjectBackground() { VersionId = versionId, Year = -1, Month = i, Date = 30 } );
      }
    }

    var existingColors = await _colorRepository.GetBackgroundColorsByVersionId( versionId );
    foreach ( var newColorData in formData.BackgroundColors ) {
      if ( newColorData.ColorId == null ) {
        var newColor = new ColorDef()
        {
          Name = newColorData.Name,
          Code = newColorData.Code,
          Type = 1,
          VersionId = versionId
        };
        var bgResult = await CreateColorDef( newColor );
        if ( bgResult.Success ) {
          newColorData.ColorId = bgResult.Content.ColorId;
        }
      }
      else {
        var existingColor = existingColors.FirstOrDefault( color => color.ColorId == newColorData.ColorId );
        if ( existingColor == null ) {
          continue;
        }
        existingColor.Name = newColorData.Name;
        existingColor.Code = newColorData.Code;
        await UpdateColorDef( existingColor );
      }
    }

    foreach ( var bg in formData.BackgroundColors ) {
      IEnumerable<DateTimeExt> dates = bg.DisplayDateRanges.SelectMany( x => x.ToDateArray() );
      foreach ( var date in dates ) {
        var newBackground = newBackgrounds.FirstOrDefault( @new => date.Equals( DateTimeExt.Create( @new ) ) );
        if ( newBackground != null ) {
          newBackground.ColorId = bg.ColorId;
        }
      }
    }

    foreach ( var background in newBackgrounds ) {
      await _backgroundRepository.Create( background );
      await _unitOfWork.CompleteAsync();
    }

    var deletingColors = existingColors.Where( color => !formData.BackgroundColors.Any( x => x.ColorId == color.ColorId ) );
    foreach ( var color in deletingColors ) {
      await DeleteColorDef( color.ColorId );
    }
  }

  private async Task<ServiceResponse<ColorDef>> CreateColorDef( ColorDef colorDef )
  {
    try {
      var newColor = await _colorRepository.Create( colorDef );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ColorDef>( newColor );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ColorDef>( $"{ColorDefNotification.ErrorSaving} {ex.Message}" );
    }
  }

  private async Task<ServiceResponse<ColorDef>> UpdateColorDef( ColorDef colorDef )
  {
    try {
      await _colorRepository.Update( colorDef );
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
      var colorDef = await _colorRepository.GetById( colorDefId );
      if ( colorDef is null )
        return new ServiceResponse<ColorDef>( ColorDefNotification.NonExisted );
      _colorRepository.Delete( colorDef );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ColorDef>( colorDef );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ColorDef>( $"{ColorDefNotification.ErrorDeleting} {ex.Message}" );
    }
  }

  public async Task<ServiceResponse<ProjectBackground>> UpdateProjectBackground( ProjectBackground projectBackground )
  {
    try {
      await _backgroundRepository.Update( projectBackground );
      await _unitOfWork.CompleteAsync();
      return new ServiceResponse<ProjectBackground>( projectBackground );
    }
    catch ( Exception ex ) {
      return new ServiceResponse<ProjectBackground>( $"{BackgroundNotification.ErrorSaving} {ex.Message}" );
    }
  }
}
