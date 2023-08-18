using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using SchedulingTool.Api.Resources;

namespace SchedulingTool.Api.ExportExcel;

public static class ExportExcel
{
  public static bool GetFile( ProjectResource resource, out string result )
  {
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    var excel = new ExcelPackage();
    var xlApp = new Application();
    string path = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, $"{Guid.NewGuid()}.xlsx" );
    try {
      #region Main view sheet
      var sheet = excel.Workbook.Worksheets.Add( "メインビュー" );
      sheet.CreateMainTableFrame( resource );
      sheet.PopulateData( resource.Grouptasks, startRow: resource.Setting.IncludeYear ? 11 : 10, numberOfMonths: resource.Backgrounds.Count / 3 );
      #endregion

      #region Custom view sheet
      foreach ( var view in resource.ViewTasks.Keys ) {
        var sheet2 = excel.Workbook.Worksheets.Add( view.ViewName );
        sheet2.CreateMainTableFrame( resource, isMainView: false, view );
        sheet2.PopulateData( resource.ViewTasks [ view ], startRow: resource.Setting.IncludeYear ? 11 : 10, numberOfMonths: resource.Backgrounds.Count / 3 );
      }
      #endregion

      #region Save to local storage
      excel.Save();
      if ( File.Exists( path ) )
        File.Delete( path );
      var fileStream = File.Create( path );
      fileStream.Close();
      File.WriteAllBytes( path, excel.GetAsByteArray() );
      excel.Dispose();
      #endregion

      #region Open again to draw chart
      var xlWorkBook = xlApp.Workbooks.Open( path );
      var xlWorkSheet = ( Worksheet ) xlWorkBook.Worksheets.get_Item( 1 );
      xlWorkSheet.DrawChart( resource.ChartStepworks );
      xlWorkSheet.AddLegend( resource.UsedColors.OrderBy( x => x.Type ).ToList(), 11 + resource.Backgrounds.Count * 2 );
      for ( int j = 2; j < resource.ViewTasks.Count + 2; j++ ) {
        var xlWorkSheet2 = ( Worksheet ) xlWorkBook.Worksheets.get_Item( j );
        xlWorkSheet2.DrawChart( resource.ViewTasks.Values.ElementAt( j - 2 ), startRow: resource.Setting.IncludeYear ? 11 : 10 );
      }
      xlWorkBook.Save();
      xlWorkBook.Close();
      xlApp.Quit();
      #endregion

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
