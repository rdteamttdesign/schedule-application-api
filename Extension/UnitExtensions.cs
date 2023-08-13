namespace SchedulingTool.Api.Extension;

public static class UnitExtensions
{
  public static double ColumnWidthToDays( this double value, double columnWidthUnit )
  {
    return value / ( columnWidthUnit / 30 );
  }

  public static double DaysToColumnWidth( this double value, double columnWidthUnit )
  {
    return value * columnWidthUnit / 30;
  }
}
