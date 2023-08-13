namespace SchedulingTool.Api.Extension;

public static class UnitExtensions
{
  public static decimal ColumnWidthToDays( this decimal value, decimal columnWidthUnit )
  {
    return value / ( columnWidthUnit / 30 );
  }

  public static decimal DaysToColumnWidth( this decimal value, decimal columnWidthUnit )
  {
    return value * columnWidthUnit / 30;
  }
}
