
using OfficeOpenXml ;
using OfficeOpenXml.Drawing ;

namespace ExcelSchedulingSample.Utils
{
  public static class ShapeUtils
  {
    public static void DeleteShape( this ExcelWorksheet workSheet, ExcelShape shape )
    {
      var shapeName = shape.Name ;
      workSheet.Drawings.Remove( shapeName ) ;
    }

    public static void DeleteAllShapes( this ExcelWorksheet workSheet )
    {
      workSheet.Drawings.Clear() ;
    }
  }
}
