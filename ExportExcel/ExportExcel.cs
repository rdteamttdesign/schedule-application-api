﻿using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Resources;

namespace SchedulingTool.Api.ExportExcel;

public static class ExportExcel
{
  public static bool GetFile(
    string projectName,
    ProjectSetting setting,
    IEnumerable<ColorDef> usedcolors,
    Dictionary<View, List<ViewTaskDetail>> viewTasks,
    IEnumerable<GroupTaskDetailResource> grouptasks,
    IEnumerable<ProjectBackgroundResource> backgrounds,
    //IEnumerable<ViewResource> viewResources,
    out string result )
  {
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    var excel = new ExcelPackage();
    var xlApp = new Application();
    string path = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, $"{Guid.NewGuid()}.xlsx" );
    try {
      #region Verify data
      var i = 10;
      var data = new List<ChartStepwork>();
      foreach ( var grouptask in grouptasks ) {
        i++;
        foreach ( var task in grouptask.Tasks ) {
          if ( task.Stepworks.Count > 1 && task.NumberOfTeam > 0 ) {
            var gap = task.Stepworks.First().Portion * task.Duration * ( task.NumberOfTeam == 0 ? 1 : ( ( setting.AmplifiedFactor - 1 ) / task.NumberOfTeam ) );
            for ( int j = 1; j < task.Stepworks.Count; j++ ) {
              task.Stepworks.ElementAt( j ).Start += gap;
              gap += task.Stepworks.ElementAt( j ).Portion * task.Duration * ( task.NumberOfTeam == 0 ? 1 : ( ( setting.AmplifiedFactor - 1 ) / task.NumberOfTeam ) );
            }
          }
          task.AmplifiedDuration = task.Duration * ( task.NumberOfTeam == 0 ? 1 : ( setting.AmplifiedFactor / task.NumberOfTeam ) );
          foreach ( var sw in task.Stepworks ) {
            var chartSw = new ChartStepwork()
            {
              StepWorkId = sw.StepworkId,
              Color = WorksheetFormater.GetColor( sw.ColorCode ),
              Start = sw.Start,
              Duration = sw.Portion * task.Duration * ( task.NumberOfTeam == 0 ? 1 : ( setting.AmplifiedFactor / task.NumberOfTeam ) ),
              RowIndex = i
            };
            foreach ( var predecessor in sw.Predecessors ) {
              chartSw.Predecessors.Add(
                new ChartPredecessor()
                {
                  RelatedProcessorStepWork = predecessor.RelatedStepworkId,
                  Lag = predecessor.Lag,
                  Type = sw.Predecessors.Count != 0 ? ( PredecessorType ) sw.Predecessors.First().Type : PredecessorType.FinishToStart
                } );
            }
            data.Add( chartSw );
          }
          i++;
        }
      }
      #endregion

      var numberOfContents = 8;
      var numberOfTasks = i - 10;
      var numberOfMonths = backgrounds.Count();

      #region Populate data, sheet 1
      var sheet = excel.Workbook.Worksheets.Add( "メインビュー" );

      // Hide gridlines
      sheet.View.ShowGridLines = false;

      // Clear shapes
      sheet.Drawings.Clear();

      sheet.CreateTitle( projectName, numberOfContents, numberOfMonths );

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

      sheet.PopulateData( grouptasks, startRow: 10, numberOfMonths );

      sheet.Cells.Style.Font.Name = "MS Gothic";
      #endregion

      #region Populate data, custom views
      foreach ( var view in viewTasks.Keys ) {
        var sheet2 = excel.Workbook.Worksheets.Add( view.ViewName );
        // Hide gridlines
        sheet2.View.ShowGridLines = false;

        // Clear shapes
        sheet2.Drawings.Clear();

        sheet2.Cells.Style.Font.Name = "MS Gothic";

        sheet2.CreateTitle( projectName, numberOfContents, numberOfMonths );

        sheet2.FormatChartTable(
          startRow: 7,
          startColumn: 2 + numberOfContents + 1,
          rowCount: numberOfTasks,
          columnCount: numberOfMonths );

        sheet2.FormatTaskTable(
          startRow: 7,
          startColumn: 2,
          rowCount: numberOfTasks,
          columnCount: numberOfContents );

        sheet2.PaintChart( startRow: 10, startColumn: 11, numberOfTasks: numberOfTasks, backgrounds );

        sheet2.PopulateData( viewTasks [ view ], startRow: 10, numberOfMonths );
      }
      #endregion

      #region 
      excel.Save();
      if ( File.Exists( path ) )
        File.Delete( path );
      var fileStream = File.Create( path );
      fileStream.Close();
      File.WriteAllBytes( path, excel.GetAsByteArray() );
      excel.Dispose();
      #endregion

      var xlWorkBook = xlApp.Workbooks.Open( path );

      var xlWorkSheet = ( Worksheet ) xlWorkBook.Worksheets.get_Item( 1 );
      xlWorkSheet.DrawChart( data );
      xlWorkSheet.AddLegend( usedcolors.OrderBy( x => x.Type ).ToList(), 10 + numberOfMonths * 6 + 1 );

      for ( int j = 2; j < viewTasks.Count + 2; j++ ) {
        var xlWorkSheet2 = ( Worksheet ) xlWorkBook.Worksheets.get_Item( j );
        xlWorkSheet2.DrawChart( viewTasks.Values.ElementAt( j - 2 ), startRow: 10 );
      }

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