using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Resources;

namespace SchedulingTool.Api.ExportExcel;

public static class ExportExcel
{
  public static bool GetFile( 
    ProjectSetting setting,
    IEnumerable<GroupTaskDetailResource> grouptasks, 
    IEnumerable<ProjectBackgroundResource> backgrounds, 
    //IEnumerable<ViewResource> viewResources,
    out string result )
  {
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    var excel = new ExcelPackage();
    var xlApp = new Application();
    string path = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, $"{Guid.NewGuid().ToString()}.xlsx" );
    try {
      //var tasks = grouptasks
      //  .SelectMany( g => g.Tasks );
      //.SelectMany( t => t.Stepworks );

      #region
      var i = 10;
      var data = new List<ChartStepwork>();
      foreach ( var grouptask in grouptasks ) {
        i++;
        foreach ( var task in grouptask.Tasks ) {
          if ( task.Stepworks.Count > 1 ) {
            var gap = task.Stepworks.First().Portion * task.Duration * ( task.NumberOfTeam == 0 ? 1 : ( setting.AmplifiedFactor - 1 ) );
            for ( int j = 1; j < task.Stepworks.Count; j++ ) {
              task.Stepworks.ElementAt( j ).Start += gap;
              gap += task.Stepworks.ElementAt( j ).Portion * task.Duration * ( task.NumberOfTeam == 0 ? 1 : ( setting.AmplifiedFactor - 1 ) );
            }
          }
          foreach ( var sw in task.Stepworks ) {
            data.Add( new ChartStepwork()
            {
              StepWorkId = sw.StepworkId,
              Color = WorksheetFormater.GetColor( "rgb(71, 71, 107)" ),
              Start = sw.Start,
              Duration = sw.Portion * task.Duration   * ( task.NumberOfTeam == 0 ? 1 : setting.AmplifiedFactor ),
              Lag = sw.Predecessors.Count != 0 ? sw.Predecessors.First().Lag : 0,
              RelatedProcessorStepWork = sw.Predecessors.Count != 0 ? sw.Predecessors.First().RelatedStepworkId : -1,
              PredecessorType = sw.Predecessors.Count != 0 ? ( PredecessorType ) sw.Predecessors.First().Type : PredecessorType.FinishToStart,
              RowIndex = i
            } );
          }
          i++;
        }
      }
      #endregion

      var sheet = excel.Workbook.Worksheets.Add( "Sheet1" );

      // Hide gridlines
      sheet.View.ShowGridLines = false;

      // Clear shapes
      sheet.Drawings.Clear();

      var numberOfContents = 8;
      var numberOfTasks = i - 10;
      var numberOfMonths = backgrounds.Count();

      sheet.CreateTitle( numberOfContents, numberOfMonths );

      sheet.FormatChartTable(
        startRow: 7,
        startColumn: 2 + numberOfContents + 1,
        rowCount: numberOfTasks,
        columnCount: numberOfMonths );

      sheet.FormatTaskTable(
        startRow: 7,
        startColumn: 2,
        rowCount: numberOfTasks,
        columnCount: numberOfContents );

      sheet.PaintChart( startRow: 10, startColumn: 11, numberOfTasks: numberOfTasks, backgrounds );

      sheet.PopulateData( grouptasks, startRow: 10 );

      sheet.Cells.Style.Font.Name = "MS Gothic";

      excel.Save();
      if ( File.Exists( path ) )
        File.Delete( path );
      var fileStream = File.Create( path );
      fileStream.Close();
      File.WriteAllBytes( path, excel.GetAsByteArray() );
      excel.Dispose();

      var xlWorkBook = xlApp.Workbooks.Open( path );
      var xlWorkSheet = ( Worksheet ) xlWorkBook.Worksheets.get_Item( 1 );
      var xlmmm = xlWorkSheet.Name;
      //var xlWorkSheet2 = ( Worksheet ) xlWorkBook.Worksheets.get_Item( 2 );

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
