namespace SchedulingTool.Api.Extension;

public static class UnitExtensions
{
  public static float ColumnWidthToDays(this float value, float columnWidthUnit)
  {
    return value / columnWidthUnit / 30;
  }

  public static float DaysToColumnWidth( this float value, float columnWidthUnit )
  {
    return value * columnWidthUnit / 30;
  }
}
