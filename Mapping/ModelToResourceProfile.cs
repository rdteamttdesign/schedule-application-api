﻿using AutoMapper;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Security.Tokens;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.Extended;

namespace SchedulingTool.Api.Mapping;

public class ModelToResourceProfile : Profile
{
  public ModelToResourceProfile()
  {
    CreateMap<AccessToken, AccessTokenResource>()
      .ForMember( a => a.AccessToken, opt => opt.MapFrom( a => a.Token ) )
      .ForMember( a => a.RefreshToken, opt => opt.MapFrom( a => a.RefreshToken.Token ) )
      .ForMember( a => a.AccessExpiration, opt => opt.MapFrom( a => a.Expiration ) )
      .ForMember( a => a.RefreshExpiration, opt => opt.MapFrom( a => a.RefreshToken.Expiration ) );

    CreateMap<Project, ProjectResource>();
    CreateMap<ColorDef, ColorDefResource>();
    CreateMap<ProjectSetting, ProjectSettingResource>();
  }
}
