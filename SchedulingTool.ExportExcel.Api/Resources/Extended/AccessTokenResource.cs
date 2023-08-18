namespace SchedulingTool.Api.Resources.Extended ;

public class AccessTokenResource
{
  public string? AccessToken { get ; set ; }
  public string? RefreshToken { get ; set ; }
  public long AccessExpiration { get; set; }
  public long RefreshExpiration { get; set; }
}