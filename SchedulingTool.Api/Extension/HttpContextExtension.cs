namespace SchedulingTool.Api.Extension;

public static class HttpContextExtension
{
  public static string GetUserName( HttpContext context )
  {
    var familyName = context.User.Claims.FirstOrDefault( x => x.Type.ToLower() == @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname" )?.Value!;
    var givenName = context.User.Claims.FirstOrDefault( x => x.Type.ToLower() == @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname" )?.Value!;
    return $"{givenName} {familyName}";
  }
}
