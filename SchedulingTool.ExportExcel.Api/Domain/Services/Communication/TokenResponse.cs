using SchedulingTool.Api.Domain.Security.Tokens ;
namespace SchedulingTool.Api.Domain.Services.Communication ;

public class TokenResponse : BaseResponse
{
  public AccessToken? Token { get; set; }

  public TokenResponse(bool success, string message, AccessToken? token): base(success, message)
  {
    Token = token;
  }
}