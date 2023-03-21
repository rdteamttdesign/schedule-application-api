using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;

namespace SchedulingTool.Api.ExportExcel;

public static class ExportExcel
{
  public static bool GetFile( out string result )
  {
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    var excel = new ExcelPackage();
    var xlApp = new Application();
    string path = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, $"{Guid.NewGuid().ToString()}.xlsx" );
    try {

      var sheet = excel.Workbook.Worksheets.Add( "Sheet1" );

      // Hide gridlines
      sheet.View.ShowGridLines = false;

      // Clear shapes
      sheet.Drawings.Clear();

      var numberContent = 8;
      var numberTask = 54;
      var numberMonth = 35;

      sheet.CreateTitle( numberContent, numberMonth );

      sheet.FormatChartTable(
        startRow: 7,
        startColumn: 2 + numberContent + 1,
        rowCount: numberTask,
        columnCount: numberMonth );

      sheet.FormatTaskTable(
        startRow: 7,
        startColumn: 2,
        rowCount: numberTask,
        columnCount: numberContent );



      excel.Save();
      if ( File.Exists( path ) )
        File.Delete( path );
      var fileStream = File.Create( path );
      fileStream.Close();
      File.WriteAllBytes( path, excel.GetAsByteArray() );
      excel.Dispose();

      result = path;
      return true;
    }
    catch ( Exception ex ) {
      excel.Dispose();
      xlApp.Quit();
      result = $"{ex.Message}: {ex.StackTrace}";
      return false;
    }
  }
}
