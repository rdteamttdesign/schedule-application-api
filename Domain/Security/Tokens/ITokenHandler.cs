using SchedulingTool.Api.Domain.Models ;

namespace SchedulingTool.Api.Domain.Security.Tokens;

public interface ITokenHandler
{
  AccessToken CreateAccessToken( User user );
  RefreshToken? TakeRefreshToken( string token );
  void RevokeRefreshToken( string token );
}
