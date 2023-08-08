using SchedulingTool.Api.Domain.Models;
using System.Text.RegularExpressions;

namespace SchedulingTool.Api.Extension;

public static class DateRangeDisplayExtension
{
  public static IList<string> ToFormatString( this IEnumerable<DateTime> value )
  {
    var orderedDateList = value.OrderBy( value => value.Year ).ThenBy( value => value.Month ).ThenBy( value => value.Date );

    var result = orderedDateList
      .Distinct()
      .GroupAdjacentBy( ( x, y ) => x.GetNextDates() == y )
      .Select( g =>
      {
        var fromDate = g.First();
        var toDate = g.Last();
        if ( fromDate.Year == -1 ) {
          return $"{fromDate.Month}/{fromDate.Date} - {toDate.Month}/{toDate.Date}";
        }
        else {
          return $"{fromDate.Year}/{fromDate.Month}/{fromDate.Date} - {toDate.Year}/{toDate.Month}/{toDate.Date}";
        }
      } );

    return result.ToList();
  }

  public static IEnumerable<DateTime> ToDateArray( this string? value )
  {
    if ( value == null ) {
      return Enumerable.Empty<DateTime>();
    }

    var dateStrings = value.Split( '-' );
    var fromDate = DateTime.Parse( dateStrings [ 0 ] );
    var toDate = DateTime.Parse( dateStrings [ 1 ] );

    if ( fromDate.Equals( DateTime.NullValue ) || toDate.Equals( DateTime.NullValue ) ) {
      return Enumerable.Empty<DateTime>();
    }

    var dateList = new List<DateTime>();
    var currentDate = fromDate;
    while ( currentDate < toDate ) {
      dateList.Add( currentDate );
      currentDate = currentDate.GetNextDates();
    }
    dateList.Add( toDate );

    return dateList;
  }

  public struct DateTime
  {
    public static DateTime NullValue = new();

    public int Year { get; set; }
    public int Month { get; set; }
    public int Date { get; set; }

    public DateTime( int year, int month, int date )
    {
      Year = year;
      Month = month;
      Date = date;
    }

    public static DateTime Create( ProjectBackground value )
    {
      return new DateTime( value.Year, value.Month, value.Date );
    }

    public static DateTime Parse( string? value )
    {
      if ( value == null ) {
        return NullValue;
      }
      var match = Regex.Match( value, @"^\d{4}\/(0?[1-9]|1[012])\/(0?[1-9]|[12][0-9]|3[0])$" );
      if ( match.Success ) {
        var arr = match.Value.Split( '/' );
        return new DateTime()
        {
          Year = int.Parse( arr [ 0 ] ),
          Month = int.Parse( arr [ 1 ] ),
          Date = int.Parse( arr [ 2 ] )
        };
      }
      match = Regex.Match( value, @"^\d+\/(0?[1-9]|[12][0-9]|3[0])$" );
      if ( match.Success ) {
        var arr = match.Value.Split( '/' );
        return new DateTime()
        {
          Year = -1,
          Month = int.Parse( arr [ 0 ] ),
          Date = int.Parse( arr [ 1 ] )
        };
      }
      return NullValue;
    }

    public DateTime GetNextDates()
    {
      var result = this;
      result.AddDate();
      return result;
    }

    private void AddDate()
    {
      if ( Date == 30 ) {
        Date = 10;
        AddMonth();
      }
      else {
        Date += 10;
      }
    }

    private void AddMonth()
    {
      if ( Month == 12 ) {
        Month = 1;
        AddYear();
      }
      else {
        Month++;
      }
    }

    private void AddYear()
    {
      Year++;
    }

    public static bool operator ==( DateTime obj1, DateTime obj2 )
    {
      return obj1.Year == obj2.Year && obj1.Month == obj2.Month && obj1.Date == obj2.Date;
    }

    public static bool operator !=( DateTime obj1, DateTime obj2 )
    {
      return obj1.Year != obj2.Year || obj1.Month != obj2.Month || obj1.Date != obj2.Date;
    }

    public static bool operator >( DateTime obj1, DateTime obj2 )
    {
      if ( obj1.Year > obj2.Year ) {
        return true;
      }
      else if ( obj1.Year == obj2.Year ) {
        if ( obj1.Month > obj2.Month ) {
          return true;
        }
        else if ( obj1.Month == obj2.Month ) {
          return obj1.Date > obj2.Date;
        }
      }
      return false;
    }

    public static bool operator <( DateTime obj1, DateTime obj2 )
    {
      if ( obj1.Year < obj2.Year ) {
        return true;
      }
      else if ( obj1.Year == obj2.Year ) {
        if ( obj1.Month < obj2.Month ) {
          return true;
        }
        else if ( obj1.Month == obj2.Month ) {
          return obj1.Date < obj2.Date;
        }
      }
      return false;
    }
  }
}
