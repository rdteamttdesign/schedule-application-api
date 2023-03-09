using Serilog;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using SchedulingTool.Api.Security.Security.Tokens;
using Microsoft.EntityFrameworkCore;
using SchedulingTool.Api.Persistence.Context;
using SchedulingTool.Api.Domain.Repositories;
using SchedulingTool.Api.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SchedulingTool.Api.Security.Tokens;
using Microsoft.IdentityModel.Tokens;
using SchedulingTool.Api.Domain.Services;
using SchedulingTool.Api.Services;
using SchedulingTool.Api.Domain.Models;
using BackgroundService = SchedulingTool.Api.Services.BackgroundService;

var builder = WebApplication.CreateBuilder( args );

var logger = new LoggerConfiguration()
  .ReadFrom.Configuration( builder.Configuration )
  .Enrich.FromLogContext()
  .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog( logger );

var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors( options =>
{
  options.AddPolicy( name: myAllowSpecificOrigins,
    corsPolicyBuilder =>
    {
      corsPolicyBuilder
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed( origin => true )
        .AllowCredentials();
    } );
} );

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddJsonOptions( x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles );
builder.Configuration.AddJsonFile(
  path: $"appsettings.{Environment.GetEnvironmentVariable( "DOTTNET_ENVIRONMENT" )}.json",
  optional: true,
  reloadOnChange: true
);

builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
      options.UseMySql( builder.Configuration.GetConnectionString( "SchedulingTool1" ),
        ServerVersion.Parse( builder.Configuration.GetSection( "ServerVersion" ).Value ) );
    } );

builder.Services.Configure<RouteOptions>( options => options.LowercaseUrls = true );
builder.Services.Configure<FormOptions>( option =>
{
  option.ValueLengthLimit = int.MaxValue;
  option.MultipartBodyLengthLimit = int.MaxValue;
  option.MemoryBufferThreshold = int.MaxValue;
} );


// Add services to the container.

//AUTH
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
//builder.Services.AddScoped<ITokenHandler, TokenHandler>();
//builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.Configure<TokenOptions>( builder.Configuration.GetSection( "TokenOptions" ) );
var tokenOptions = builder.Configuration.GetSection( "TokenOptions" ).Get<TokenOptions>();
var signingConfigurations = new SigningConfigurations( tokenOptions.Secret );
builder.Services.AddSingleton( signingConfigurations );
builder.Services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme )
  .AddJwtBearer( jwtBearerOptions =>
  {
    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters()
    {
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      ValidIssuer = tokenOptions.Issuer,
      ValidAudience = tokenOptions.Audience,
      IssuerSigningKey = signingConfigurations.SecurityKey,
      ClockSkew = TimeSpan.Zero
    };

    //jwtBearerOptions.Events = new JwtBearerEvents
    //{
    //  OnMessageReceived = context =>
    //  {
    //    var accessToken = context.Request.Query [ "access_token" ];
    //    var path = context.HttpContext.Request.Path;
    //    if ( !string.IsNullOrEmpty( accessToken ) &&
    //         ( path.StartsWithSegments( $"/{hubConnectionName}" ) ) ) {
    //      context.Token = accessToken;
    //    }
    //    return Task.CompletedTask;
    //  }
    //};
  } );

//builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();

builder.Services.AddScoped<IProjectSettingRepository, ProjectSettingRepository>();
builder.Services.AddScoped<IProjectSettingService, ProjectSettingService>();

builder.Services.AddScoped<IBackgroundRepository, BackgroundRepository>();
builder.Services.AddScoped<IBackgroundService, BackgroundService>();

builder.Services.AddScoped<IColorDefRepository, ColorDefRepository>();
builder.Services.AddScoped<IColorDefService, ColorDefService>();

builder.Services.AddScoped<IGroupTaskRepository, GroupTaskRepository>();
builder.Services.AddScoped<IGroupTaskService, GroupTaskService>();

builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

builder.Services.AddScoped<IStepworkRepository, StepworkRepository>();
builder.Services.AddScoped<IStepworkService, StepworkService>();

builder.Services.AddScoped<IPredecessorRepository, PredecessorRepository>();
builder.Services.AddScoped<IPredecessorService, PredecessorService>();

builder.Services.AddScoped<IPredecessorTypeRepository, PredecessorTypeRepository>();
builder.Services.AddScoped<IPredecessorTypeService, PredecessorTypeService>();

builder.Services.AddScoped<IViewRepository, ViewRepository>();
builder.Services.AddScoped<IViewService, ViewService>();

builder.Services.AddAutoMapper( AppDomain.CurrentDomain.GetAssemblies() );

var app = builder.Build();

app.UseCors( myAllowSpecificOrigins );
app.Use( async ( context, next ) =>
{
  context.Response.Headers.Add( "Access-Control-Allow-Origin", "*" );
  context.Response.Headers.Add( "Access-Control-Allow-Methods", "GET, POST, PUT, PATCH, DELETE, OPTIONS" );
  context.Response.Headers.Add( "Access-Control-Allow-Headers", "Origin,DNT,X-Mx-ReqToken,Keep-Alive,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type" );
  await next();
} );

// Configure the HTTP request pipeline.
if ( app.Environment.IsDevelopment() || app.Environment.IsStaging() ) {
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();




//var builder = WebApplication.CreateBuilder( args );

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if ( app.Environment.IsDevelopment() ) {
//  app.UseSwagger();
//  app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
