using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using SchedulingTool.Api.Resources;

namespace SchedulingTool.Api.ExportExcel;

public static class ExportExcel
{
  public static bool GetFile( IEnumerable<GroupTaskDetailResource> grouptasks, out string result )
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

      sheet.PaintChart(
        startRow: 10, startColumn: 11, numberOfTasks: 20,
        new List<BackgroundColorResource>()
        {
          new BackgroundColorResource()
          {
            Code = "rgb(100,3,225)",
            Months = new [] {1, 3,5, 15, 6}
          },
          new BackgroundColorResource()
          {
            Code = "rgb(2,53,102)",
            Months = new [] {2,8,10}
          }
        } );

      sheet.PopulateData( grouptasks, startRow: 10 );

      excel.Save();
      if ( File.Exists( path ) )
        File.Delete( path );
      var fileStream = File.Create( path );
      fileStream.Close();
      File.WriteAllBytes( path, excel.GetAsByteArray() );
      excel.Dispose();

      var xlWorkBook = xlApp.Workbooks.Open( path );
      var xlWorkSheet = ( Worksheet ) xlWorkBook.Worksheets.get_Item( 1 );
      var xlWorkSheet2 = ( Worksheet ) xlWorkBook.Worksheets.get_Item( 2 );

      var data = grouptasks
        .SelectMany( g => g.Tasks )
        .SelectMany( t => t.Stepworks )
        .Select( s => new ChartStepwork()
        {
          StepWorkId = s.StepworkId,
          Color = WorksheetFormater.GetColor( s.ColorCode ),
          Duration = s.Portion,
          Lag = s.Predecessors.First().Lag,
          RelatedProcessorStepWork = s.Predecessors.First().StepworkId
        } );
      xlWorkSheet.DrawChart( data );

      xlWorkBook.Save();
      xlWorkBook.Close();
      xlApp.Quit();

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
