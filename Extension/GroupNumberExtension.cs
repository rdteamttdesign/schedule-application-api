namespace SchedulingTool.Api.Extension;

public static class GroupNumberExtension
{
  public static string ToFormatString( this IEnumerable<int> value )
  {
    return string.Join( ",", value
      .Distinct()
      .OrderBy( x => x )
      .GroupAdjacentBy( ( x, y ) => x + 1 == y )
      .Select( g => new int [] { g.First(), g.Last() }.Distinct() )
      .Select( g => string.Join( "-", g ) ) );
  }

  public static IEnumerable<int> ToNumberArray( this string? value )
  {
    if ( value == null ) {
      return Enumerable.Empty<int>();
    }
    var groupsOfNumber = value.Split( "," );
    var result = new List<int>();
    foreach ( var group in groupsOfNumber ) {
      if ( group.Contains( '-' ) ) {
        var fromTo = group.Split( "-" );
        int.TryParse( fromTo [ 0 ], out var from );
        int.TryParse( fromTo [ 1 ], out var to );
        for ( int i = from; i <= to; i++ ) {
          result.Add( i );
        }
      }
      else {
        if ( int.TryParse( group!, out var number ) ) {
          result.Add( number );
        }
      }
    }
    return result;
  }
}
