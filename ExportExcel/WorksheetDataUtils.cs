using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Models.Enum;
using SchedulingTool.Api.Resources;
using System.Drawing;
using Range = Microsoft.Office.Interop.Excel.Range;
using Shape = Microsoft.Office.Interop.Excel.Shape;

namespace SchedulingTool.Api.ExportExcel;

public static class WorksheetContentUtils
{
  private const double ALMOST_ZERO = 10e-6;
  public static void PopulateData( this ExcelWorksheet ws, IEnumerable<GroupTaskDetailResource> grouptasks, int startRow, int numberOfMonths )
  {
    var index = 1;
    foreach ( var group in grouptasks ) {
      ws.Cells [ startRow, 3 ].Value = group.GroupTaskName;
      ws.Cells [ startRow, 3 ].Style.Font.Bold = true;
      ws.Cells [ startRow, 3 ].Style.WrapText = false;
      startRow++;
      foreach ( var task in group.Tasks ) {
        ws.Cells [ startRow, 2 ].Value = index;

        ws.Cells [ startRow, 3 ].Value = task.TaskName;
        ws.Cells [ startRow, 3 ].Style.Indent = 1;

        ws.Cells [ startRow, 4 ].Value = task.Description;

        ws.Cells [ startRow, 5 ].Value = task.Duration;
        ws.Cells [ startRow, 5 ].Style.Numberformat.Format = "#,###0.0 日";

        if ( task.NumberOfTeam > 0 ) {
          ws.Cells [ startRow, 6 ].Value = task.NumberOfTeam;
          ws.Cells [ startRow, 6 ].Style.Numberformat.Format = "#,### 班";
        }
        else {
          ws.Cells [ startRow, 6 ].Value = "-";
        }

        ws.Cells [ startRow, 7 ].Value = task.AmplifiedDuration;
        ws.Cells [ startRow, 7 ].Style.Numberformat.Format = "#,###0.0 日";

        var installStepWork = task.Stepworks.FirstOrDefault( sw => sw.ColorDetail.ColorMode == ColorMode.Install );
        var removalStepWork = task.Stepworks.FirstOrDefault( sw => sw.ColorDetail.ColorMode == ColorMode.Removal );
        if ( installStepWork != null && removalStepWork != null ) {
          ws.Cells [ startRow, 8 ].Value = installStepWork.Portion * task.AmplifiedDuration;
          ws.Cells [ startRow, 8 ].Style.Numberformat.Format = "#,###0.0 日";
          ws.Cells [ startRow, 9 ].Value = removalStepWork.Portion * task.AmplifiedDuration;
          ws.Cells [ startRow, 9 ].Style.Numberformat.Format = "#,###0.0 日";
        }

        ws.Cells [ startRow, numberOfMonths * 6 + 11 ].Value = task.Note;

        index++;
        startRow++;
      }
    }
  }

  public static void DrawChart( this Worksheet xlWorkSheet, IEnumerable<ChartStepwork> chartStepwork )
  {
    var k = 1.8;
    var startColumn = 11;
    //var startRow = 1;
    foreach ( var stepwork in chartStepwork ) {
      // CreateShape
      var offset = stepwork.Start * k; //CalculateOffsetStepWork( stepwork, chartStepwork );
      // Get position cell top, left
      Range rangeColumnRowStart = xlWorkSheet.Range [
        xlWorkSheet.Cells [ stepwork.RowIndex, startColumn ],
        xlWorkSheet.Cells [ stepwork.RowIndex, startColumn ] ];

      var width = stepwork.Duration * k;
      var rect = xlWorkSheet.Shapes.AddShape(
        MsoAutoShapeType.msoShapeRectangle,
        Convert.ToSingle( rangeColumnRowStart.Left ) + ( float ) offset,
        Convert.ToSingle( rangeColumnRowStart.Top ) + 7.5f, ( float ) width, 5f );
      if ( rect != null ) {
        rect.Fill.ForeColor.RGB = ColorTranslator.ToOle( stepwork.Color );
        rect.Line.Visible = MsoTriState.msoFalse;
        stepwork.Shape = rect;
      }
    }

    foreach ( var stepWork in chartStepwork ) {
      foreach ( var predecessor in stepWork.Predecessors ) {
        var related = chartStepwork.FirstOrDefault( x => x.StepWorkId == predecessor.RelatedProcessorStepWork );
        if ( related == null )
          continue;
        EditConnectorShape( xlWorkSheet, stepWork, related, predecessor );
      }
    }
  }

  private static void EditConnectorShape(
    Worksheet xlWorkSheet,
    ChartStepwork stepwork,
    ChartStepwork relatedStepwork,
    ChartPredecessor predecessor )
  {
    switch ( predecessor.Type ) {
      case PredecessorType.FinishToStart:
        var isClosed = Math.Abs( stepwork.Start + stepwork.Duration - relatedStepwork.Start ) < ALMOST_ZERO;
        var connectorType = isClosed ? MsoConnectorType.msoConnectorStraight : MsoConnectorType.msoConnectorElbow;
        var connection = xlWorkSheet.Shapes.AddConnector( connectorType, 1, 1, 1, 1 );
        connection.Line.ForeColor.RGB = ColorTranslator.ToOle( relatedStepwork.Color );
        connection.ConnectorFormat.BeginConnect( stepwork.Shape, 4 );
        connection.ConnectorFormat.EndConnect( relatedStepwork.Shape, 2 );
        if ( connectorType == MsoConnectorType.msoConnectorElbow ) {
          var relatedDistance = relatedStepwork.Start - stepwork.Start - stepwork.Duration;
          if ( Math.Abs( relatedDistance ) > ALMOST_ZERO && relatedDistance < 0 ) {
            connection.Adjustments [ 1 ] = -0.1f;
            connection.Adjustments [ 3 ] = 1f;
          }
          else
            connection.Adjustments [ 1 ] = 1f;
        }
        connection.Line.Weight = 1f;
        break;
      case PredecessorType.StartToStart:
        isClosed = Math.Abs( stepwork.Start - relatedStepwork.Start ) < ALMOST_ZERO;
        connectorType = isClosed ? MsoConnectorType.msoConnectorStraight : MsoConnectorType.msoConnectorElbow;
        connection = xlWorkSheet.Shapes.AddConnector( connectorType, 1, 1, 1, 1 );
        connection.Line.ForeColor.RGB = ColorTranslator.ToOle( relatedStepwork.Color );
        connection.ConnectorFormat.BeginConnect( relatedStepwork.Shape, 2 );
        connection.ConnectorFormat.EndConnect( stepwork.Shape, 2 );
        if ( connectorType == MsoConnectorType.msoConnectorElbow ) {
          var relatedDistance = stepwork.Start - relatedStepwork.Start;
          if ( Math.Abs( relatedDistance ) > ALMOST_ZERO && relatedDistance > 0 )
            connection.Adjustments [ 1 ] = 0f;
          else
            connection.Adjustments [ 1 ] = 1f;
        }
        connection.Line.Weight = 1f;
        break;
      case PredecessorType.FinishToFinish:
        isClosed = Math.Abs( stepwork.Start + stepwork.Duration - relatedStepwork.Start - relatedStepwork.Duration ) < ALMOST_ZERO;
        connectorType = isClosed ? MsoConnectorType.msoConnectorStraight : MsoConnectorType.msoConnectorElbow;
        connection = xlWorkSheet.Shapes.AddConnector( connectorType, 1, 1, 1, 1 );
        connection.Line.ForeColor.RGB = ColorTranslator.ToOle( relatedStepwork.Color );
        connection.ConnectorFormat.BeginConnect( relatedStepwork.Shape, 4 );
        connection.ConnectorFormat.EndConnect( stepwork.Shape, 4 );
        if ( connectorType == MsoConnectorType.msoConnectorElbow ) {
          var relatedDistance = relatedStepwork.Start + relatedStepwork.Duration - stepwork.Start - stepwork.Duration;
          if ( Math.Abs( relatedDistance ) > ALMOST_ZERO && relatedDistance > 0 )
            connection.Adjustments [ 1 ] = 0f;
          else
            connection.Adjustments [ 1 ] = 1f;
        }
        connection.Line.Weight = 1f;
        break;
      default:
        break;
    }
  }

  public static void PopulateData( this ExcelWorksheet worksheet, IEnumerable<ViewTaskDetail> tasks, int startRow, int numberOfMonths )
  {
    var tasksByGroup = tasks.GroupBy( x => x.GroupTaskName );
    var i = 1;
    foreach ( var group in tasksByGroup ) {
      worksheet.Cells [ startRow, 3 ].Value = group.Key;
      worksheet.Cells [ startRow, 3 ].Style.Font.Bold = true;
      worksheet.Cells [ startRow, 3 ].Style.WrapText = false;
      startRow++;
      foreach ( var task in group.OrderBy( t => t.DisplayOrder ) ) {
        worksheet.Cells [ startRow, 2 ].Value = i;
        worksheet.Cells [ startRow, 3 ].Value = task.TaskName;
        worksheet.Cells [ startRow, 4 ].Value = task.Description;
        worksheet.Cells [ startRow, 3 ].Style.Indent = 1;
        worksheet.Cells [ startRow, 7 ].Value = task.Duration;
        worksheet.Cells [ startRow, 7 ].Style.Numberformat.Format = "#,###0.0 日";
        worksheet.Cells [ startRow, numberOfMonths * 6 + 11 ].Value = task.Note;
        worksheet.Columns [ numberOfMonths * 6 + 11 ].AutoFit();
        startRow++;
        i++;
      }
    }
  }

  public static void DrawChart( this Worksheet xlWorkSheet, IEnumerable<ViewTaskDetail> tasks, int startRow )
  {
    var color = Color.Violet;
    var columnStart = 11;
    var k = 1.8;
    var tasksByGroup = tasks.GroupBy( x => x.GroupTaskName );
    foreach ( var group in tasksByGroup ) {
      startRow++;
      foreach ( var task in group.OrderBy( t => t.DisplayOrder ) ) {
        // CreateShape
        var offset = task.MinStart * k;
        // Get position cell top, left
        Range rangeColumnRowStart = xlWorkSheet.Range [
          xlWorkSheet.Cells [ startRow, columnStart ],
          xlWorkSheet.Cells [ startRow, columnStart ] ];
        if ( task.Duration > 0 ) {
          var width = task.Duration * k;
          var rect = xlWorkSheet.Shapes.AddShape(
            MsoAutoShapeType.msoShapeRectangle,
            Convert.ToSingle( rangeColumnRowStart.Left ) + ( float ) offset,
            Convert.ToSingle( rangeColumnRowStart.Top ) + 7.5f, ( float ) width, 5 );
          if ( rect != null ) {
            rect.Fill.ForeColor.RGB = ColorTranslator.ToOle( color );
            rect.Line.Visible = MsoTriState.msoFalse;
          }
        }
        startRow++;
      }
    }
  }

  public static void AddLegend( this Worksheet xlWorkSheet, IList<ColorDef> colors, int endColumnIndex )
  {
    int space = 9;

    int startRow = 3;
    int numberOfStack = ( int ) Math.Ceiling( ( double ) colors.Count / 3 );
    int startColumn = endColumnIndex - 5 - ( numberOfStack - 1 ) * space;

    int iColor = 0;
    for ( int i = 0; i < numberOfStack - 1; i++ ) {
      for ( int j = 0; j < 3; j++ ) {
        var color = colors [ iColor ];
        if ( color.Type == 1 ) {
          var shape = xlWorkSheet.Shapes.AddShape(
            MsoAutoShapeType.msoShapeRectangle,
            Convert.ToSingle( xlWorkSheet.Range [
              xlWorkSheet.Cells [ startRow + j, startColumn - i * space + 5 ],
              xlWorkSheet.Cells [ startRow + j, startColumn - i * space + 5 ] ].Left ),
            Convert.ToSingle( xlWorkSheet.Range [
              xlWorkSheet.Cells [ startRow + j, startColumn - i * space + 5 ],
              xlWorkSheet.Cells [ startRow + j, startColumn - i * space + 5 ] ].Top ) + 2f,
            15 * 1.808f, 10.5f );
          shape.Fill.ForeColor.RGB = ColorTranslator.ToOle( WorksheetFormater.GetColor( color.Code ) );
          shape.Line.Visible = MsoTriState.msoFalse;
          xlWorkSheet.Cells [ startRow + j, startColumn - i * space ] = color.Name;
        }
        else {
          var shape = xlWorkSheet.Shapes.AddShape(
            MsoAutoShapeType.msoShapeRectangle,
            Convert.ToSingle( xlWorkSheet.Range [
              xlWorkSheet.Cells [ startRow + j, startColumn - i * space + 5 ],
              xlWorkSheet.Cells [ startRow + j, startColumn - i * space + 5 ] ].Left ),
            Convert.ToSingle( xlWorkSheet.Range [
              xlWorkSheet.Cells [ startRow + j, startColumn - i * space + 5 ],
              xlWorkSheet.Cells [ startRow + j, startColumn - i * space + 5 ] ].Top ) + 5f,
            15 * 1.808f, 5 );
          shape.Fill.ForeColor.RGB = ColorTranslator.ToOle( WorksheetFormater.GetColor( color.Code ) );
          shape.Line.Visible = MsoTriState.msoFalse;
          xlWorkSheet.Cells [ startRow + j, startColumn - i * space ] = color.Name;
        }
        iColor++;
      }
    }

    for ( int j = 0; j < 3; j++ ) {
      var color = colors [ iColor ];
      if ( color.Type == 1 ) {
        var shape = xlWorkSheet.Shapes.AddShape(
          MsoAutoShapeType.msoShapeRectangle,
          Convert.ToSingle( xlWorkSheet.Range [
            xlWorkSheet.Cells [ startRow + j, endColumnIndex ],
            xlWorkSheet.Cells [ startRow + j, endColumnIndex ] ].Left ),
          Convert.ToSingle( xlWorkSheet.Range [
            xlWorkSheet.Cells [ startRow + j, endColumnIndex ],
            xlWorkSheet.Cells [ startRow + j, endColumnIndex ] ].Top ) + 2f,
          15 * 1.808f, 10.5f );
        shape.Fill.ForeColor.RGB = ColorTranslator.ToOle( WorksheetFormater.GetColor( color.Code ) );
        shape.Line.Visible = MsoTriState.msoFalse;
        xlWorkSheet.Cells [ startRow + j, endColumnIndex - 4 ] = color.Name;
      }
      else {
        var shape = xlWorkSheet.Shapes.AddShape(
          MsoAutoShapeType.msoShapeRectangle,
          Convert.ToSingle( xlWorkSheet.Range [
            xlWorkSheet.Cells [ startRow + j, endColumnIndex ],
            xlWorkSheet.Cells [ startRow + j, endColumnIndex ] ].Left ),
          Convert.ToSingle( xlWorkSheet.Range [
            xlWorkSheet.Cells [ startRow + j, endColumnIndex ],
            xlWorkSheet.Cells [ startRow + j, endColumnIndex ] ].Top ) + 5f,
          15 * 1.808f, 5 );
        shape.Fill.ForeColor.RGB = ColorTranslator.ToOle( WorksheetFormater.GetColor( color.Code ) );
        shape.Line.Visible = MsoTriState.msoFalse;
        xlWorkSheet.Cells [ startRow + j, endColumnIndex - 4 ] = color.Name;
      }
      iColor++;
      if ( iColor >= colors.Count ) {
        break;
      }
    }

    xlWorkSheet.Cells [ 4, startColumn - 5 ] = "凡例:";
  }
}

public class ChartStepwork
{
  public long TaskId { get; set; }
  public long StepWorkId { get; set; }
  public double Start { get; set; }
  public double Duration { get; set; }
  public List<ChartPredecessor> Predecessors { get; set; } = new List<ChartPredecessor>();
  public int RowIndex { get; set; }
  public Shape? Shape { get; set; }
  public Color Color { get; set; } = Color.AliceBlue;
}

public class ChartPredecessor
{
  public long RelatedProcessorStepWork { get; set; }
  public double Lag { get; set; }
  public PredecessorType Type { get; set; }
}

public class ChartTaskSummary
{
  public int RowIndex { get; set; } = 0;
  public int? TaskId { get; set; }
  public string TaskName { get; set; } = null!;
  public string? Description { get; set; }
  public double? NumberDay2 { get; set; }
  public double? Duration { get; set; }
  public double? Offset { get; set; }
  public string? Note { get; set; }
}

public enum PredecessorType
{
  FinishToStart = 1,
  StartToStart = 2,
  FinishToFinish = 3
}