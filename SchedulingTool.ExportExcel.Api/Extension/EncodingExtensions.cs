using Newtonsoft.Json;
using System.Text;

namespace SchedulingTool.Api.Extension
{
  public static class EncodingExtensions
  {
    public static string EncodeBase64( this string value )
    {
      var valueBytes = Encoding.UTF8.GetBytes( value );
      return Convert.ToBase64String( valueBytes );
    }

    public static string DecodeBase64( this string value )
    {
      var valueBytes = Convert.FromBase64String( value );
      return Encoding.UTF8.GetString( valueBytes );
    }

    public static string EncodeBase64<T>( this T value )
    {
      var json = JsonConvert.SerializeObject( value );
      var valueBytes = Encoding.UTF8.GetBytes( json );
      return Convert.ToBase64String( valueBytes );
    }

    public static string EncodeSafeUrlBase64<T>( this T value )
    {
      return value.EncodeBase64().TrimEnd( '=' );
    }

    public static string EncodeSafeUrlBase64<T>( this string value )
    {
      return value.EncodeBase64().TrimEnd( '=' );
    }

    public static T? DecodeBase64<T>( this string value )
    {
      var valueBytes = Convert.FromBase64String( value );
      var json = Encoding.UTF8.GetString( valueBytes );
      return JsonConvert.DeserializeObject<T>( json );
    }
  }
}
