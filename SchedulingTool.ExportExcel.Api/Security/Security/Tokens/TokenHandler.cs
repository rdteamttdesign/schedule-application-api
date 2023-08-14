using Microsoft.Extensions.Options;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Security.Hashing;
using SchedulingTool.Api.Domain.Security.Tokens;
using SchedulingTool.Api.Security.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SchedulingTool.Api.Security.Security.Tokens;

public class TokenHandler : ITokenHandler
{
  private static readonly ISet<RefreshToken> RefreshTokens = new HashSet<RefreshToken>();

  private readonly TokenOptions _tokenOptions;
  private readonly SigningConfigurations _signingConfigurations;
  private readonly IPasswordHasher _passwordHasher;

  public TokenHandler( IOptions<TokenOptions> tokenOptionSnapshot, SigningConfigurations signingConfigurations, IPasswordHasher passwordHasher )
  {
    _passwordHasher = passwordHasher;
    _tokenOptions = tokenOptionSnapshot.Value;
    _signingConfigurations = signingConfigurations;
  }

  public AccessToken CreateAccessToken( User user )
  {
    var refreshToken = BuildRefreshToken();
    var accessToken = BuildAccessToken( user, refreshToken );
    RefreshTokens.Add( refreshToken );

    return accessToken;
  }

  public RefreshToken? TakeRefreshToken( string token )
  {
    if ( string.IsNullOrEmpty( token ) ) return null;

    var refreshToken = RefreshTokens.FirstOrDefault( t => t.Token == token );
    if ( refreshToken != null )
      RefreshTokens.Remove( refreshToken );

    return refreshToken;
  }

  public void RevokeRefreshToken( string token )
  {
    TakeRefreshToken( token );
  }

  private RefreshToken BuildRefreshToken()
  {
    var refreshToken = new RefreshToken
    (
        token: _passwordHasher.HashPassword( Guid.NewGuid().ToString() ),
        expiration: DateTime.UtcNow.AddSeconds( _tokenOptions.RefreshTokenExpiration ).Ticks
    );

    return refreshToken;
  }

  private AccessToken BuildAccessToken( User user, RefreshToken refreshToken )
  {
    var accessTokenExpiration = DateTime.UtcNow.AddSeconds( _tokenOptions.AccessTokenExpiration );

    var securityToken = new JwtSecurityToken
    (
        issuer: _tokenOptions.Issuer,
        audience: _tokenOptions.Audience,
        claims: GetClaims( user ),
        expires: accessTokenExpiration,
        notBefore: DateTime.UtcNow,
        signingCredentials: _signingConfigurations.SigningCredentials
    );

    var handler = new JwtSecurityTokenHandler();
    var accessToken = handler.WriteToken( securityToken );

    return new AccessToken( accessToken, accessTokenExpiration.Ticks, refreshToken );
  }

  private IEnumerable<Claim> GetClaims( User user )
  {
    var claims = new List<Claim>
          {
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
              new Claim(JwtRegisteredClaimNames.Sid, user.UserId.ToString()),
              new Claim(JwtRegisteredClaimNames.Email, user.UserName),
              new Claim(JwtRegisteredClaimNames.Aud, user.LastName),
          };
    
    return claims;
  }
}
