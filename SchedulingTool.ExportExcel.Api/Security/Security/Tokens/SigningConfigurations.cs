using Microsoft.IdentityModel.Tokens ;
using System.Text ;

namespace SchedulingTool.Api.Security.Security.Tokens;

public class SigningConfigurations
{
  public SecurityKey SecurityKey { get; }
  public SigningCredentials SigningCredentials { get; }

  public SigningConfigurations( string key )
  {
    var keyBytes = Encoding.ASCII.GetBytes( key );
    SecurityKey = new SymmetricSecurityKey( keyBytes );
    SigningCredentials = new SigningCredentials( SecurityKey, SecurityAlgorithms.HmacSha256Signature );
  }
}
