using AutoMapper;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Models.Extended;
using SchedulingTool.Api.Domain.Security.Tokens;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.Extended;
using SchedulingTool.Api.Resources.FormBody.projectdetail;
using SchedulingTool.Api.Resources.projectdetail;
using Task = SchedulingTool.Api.Domain.Models.Task;
using Version = SchedulingTool.Api.Domain.Models.Version;

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

    CreateMap<Version, VersionResource>();
    CreateMap<Project, ProjectResource>();
    CreateMap<ProjectVersionDetails, VersionResource>();
    CreateMap<ColorDef, ColorDefResource>();
    CreateMap<ColorDef, BackgroundColorResource>();
    CreateMap<ProjectSetting, ProjectSettingResource>();
    CreateMap<ProjectBackground, BackgroundResource>();
    CreateMap<View, ViewResource>();
    CreateMap<ViewTask, ViewTaskResource>()
      .ForMember( dest => dest.Id, opt => opt.MapFrom( src => src.LocalTaskId ) );

    CreateMap<GroupTask, GroupTaskDetailResource>();
    CreateMap<Task, TaskDetailResource>();
    CreateMap<Stepwork, StepworkDetailResource>();
    CreateMap<Predecessor, PredecessorDetailResource>();

    CreateMap<GroupTask, GroupTaskResource>()
      //.ForMember( dest => dest.Start, opt => opt.MapFrom( src => src.Name ) )
      //.ForMember( dest => dest.Duration, opt => opt.MapFrom( src => src.Name ) )
      .ForMember( dest => dest.Name, opt => opt.MapFrom( src => src.GroupTaskName ) )
      .ForMember( dest => dest.Id, opt => opt.MapFrom( src => src.LocalId ) )
      .ForMember( dest => dest.Type, opt => opt.MapFrom( src => "project" ) )
      .ForMember( dest => dest.HideChildren, opt => opt.MapFrom( src => src.HideChidren ) )
      .ForMember( dest => dest.DisplayOrder, opt => opt.MapFrom( src => src.Index ) );

    CreateMap<Task, TaskResource>()
      //.ForMember( dest => dest.Start, opt => opt.MapFrom( src => src. ) )
      .ForMember( dest => dest.Duration, opt => opt.MapFrom( src => src.Duration ) )
      .ForMember( dest => dest.Name, opt => opt.MapFrom( src => src.TaskName ) )
      .ForMember( dest => dest.Id, opt => opt.MapFrom( src => src.LocalId ) )
      .ForMember( dest => dest.Type, opt => opt.MapFrom( src => "task" ) )
      .ForMember( dest => dest.Detail, opt => opt.MapFrom( src => src.Description ) )
      .ForMember( dest => dest.GroupId, opt => opt.MapFrom( src => src.GroupTaskLocalId ) )
      .ForMember( dest => dest.DisplayOrder, opt => opt.MapFrom( src => src.Index ) )
      .ForMember( dest => dest.Note, opt => opt.MapFrom( src => src.Note ) )
      //.ForMember( dest => dest.ColorId, opt => opt.MapFrom( src => src.GroupTaskName ) )
      .ForMember( dest => dest.GroupsNumber, opt => opt.MapFrom( src => src.NumberOfTeam ) )
      .ForMember( dest => dest.Stepworks, opt => opt.Ignore() );

    CreateMap<Stepwork, StepworkResource>()
      .ForMember( dest => dest.Start, opt => opt.MapFrom( src => src.Start ) )
      .ForMember( dest => dest.Duration, opt => opt.MapFrom( src => src.Duration ) )
      .ForMember( dest => dest.PercentStepWork, opt => opt.MapFrom( src => src.Portion ) )
      .ForMember( dest => dest.Name, opt => opt.MapFrom( src => src.Name ) )
      .ForMember( dest => dest.ParentTaskId, opt => opt.MapFrom( src => src.TaskLocalId ) )
      .ForMember( dest => dest.Id, opt => opt.MapFrom( src => src.LocalId ) )
      .ForMember( dest => dest.Type, opt => opt.MapFrom( src => "task" ) )
      //.ForMember( dest => dest.GroupId, opt => opt.MapFrom( src => src. ) )
      .ForMember( dest => dest.DisplayOrder, opt => opt.MapFrom( src => src.Index ) );

    CreateMap<Predecessor, PredecessorResource>()
      .ForMember( dest => dest.Type, opt => opt.MapFrom( src => src.Type == 1 ? "FS" : ( src.Type == 2 ? "SS" : "FF" ) ) )
      .ForMember( dest => dest.Id, opt => opt.MapFrom( src => src.RelatedStepworkLocalId ) )
      .ForMember( dest => dest.LagDays, opt => opt.MapFrom( src => src.Lag ) );
  }
}
