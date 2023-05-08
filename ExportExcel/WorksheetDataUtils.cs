using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Resources;
using System.Drawing;
using Range = Microsoft.Office.Interop.Excel.Range;
using Shape = Microsoft.Office.Interop.Excel.Shape;

namespace SchedulingTool.Api.ExportExcel;

public static class WorksheetContentUtils
{
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
        ws.Cells [ startRow, 5 ].Style.Numberformat.Format = "#,###.0 日";

        if ( task.NumberOfTeam > 0 ) {
          ws.Cells [ startRow, 6 ].Value = task.NumberOfTeam;
          ws.Cells [ startRow, 6 ].Style.Numberformat.Format = "#,### 班";
        }
        else {
          ws.Cells [ startRow, 6 ].Value = "-";
        }

        ws.Cells [ startRow, 7 ].Value = task.AmplifiedDuration;
        ws.Cells [ startRow, 7 ].Style.Numberformat.Format = "#,###.00 日";

        if ( task.Stepworks.Count == 2 ) {
          ws.Cells [ startRow, 8 ].Value = $"{Math.Round( task.Stepworks.ElementAt( 0 ).Portion * task.AmplifiedDuration, 1 )}日";
          ws.Cells [ startRow, 9 ].Value = $"{Math.Round( task.Stepworks.ElementAt( 1 ).Portion * task.AmplifiedDuration, 1 )}日";
        }

        ws.Cells [ startRow, numberOfMonths * 6 + 11 ].Value = task.Note;

        index++;
        startRow++;
      }
    }
  }

  public static void DrawChart( this Worksheet xlWorkSheet, IEnumerable<ChartStepwork> chartStepwork )
  {
    var k = 1.8f;
    var color = Color.Blue;
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
        Convert.ToSingle( rangeColumnRowStart.Left ) + offset,
        Convert.ToSingle( rangeColumnRowStart.Top ) + 7.5f, width, 5f );
      if ( rect != null ) {
        rect.Fill.ForeColor.RGB = ColorTranslator.ToOle( stepwork.Color );
        rect.Line.Visible = MsoTriState.msoFalse;
        stepwork.Shape = rect;
      }
    }

    foreach ( var stepWork in chartStepwork ) {
      foreach ( var predecessor in stepWork.Predecessors ) {
        if ( predecessor.Lag == 0 ) {
          var related = chartStepwork.FirstOrDefault( x => x.StepWorkId == predecessor.RelatedProcessorStepWork );
          var connection = xlWorkSheet.Shapes.AddConnector( MsoConnectorType.msoConnectorStraight, 1, 1, 1, 1 );
          connection.Line.ForeColor.RGB = ColorTranslator.ToOle( stepWork.Color );

          if ( related?.Shape == null )
            continue;

          switch ( stepWork.PredecessorType ) {
            case PredecessorType.FinishToStart:
              connection.ConnectorFormat.BeginConnect( stepWork.Shape, 4 );
              connection.ConnectorFormat.EndConnect( related.Shape, 2 );
              break;
            case PredecessorType.StartToStart:
              connection.ConnectorFormat.BeginConnect( related.Shape, 2 );
              connection.ConnectorFormat.EndConnect( stepWork.Shape, 2 );
              break;
            case PredecessorType.FinishToFinish:
              connection.ConnectorFormat.BeginConnect( related.Shape, 4 );
              connection.ConnectorFormat.EndConnect( stepWork.Shape, 4 );
              break;
            default:
              break;
          }

          connection.Line.Weight = 1f;
          connection.Line.BeginArrowheadLength = MsoArrowheadLength.msoArrowheadLengthMedium;
          connection.Line.BeginArrowheadWidth = MsoArrowheadWidth.msoArrowheadNarrow;
          // connection.Line.EndArrowheadStyle = MsoArrowheadStyle.msoArrowheadTriangle;
        }
        else {
          var related = chartStepwork.FirstOrDefault( x => x.StepWorkId == predecessor.RelatedProcessorStepWork );
          var connection = xlWorkSheet.Shapes.AddConnector( MsoConnectorType.msoConnectorElbow, 1, 1, 1, 1 );
          connection.Line.ForeColor.RGB = ColorTranslator.ToOle( stepWork.Color );

          if ( related?.Shape == null )
            continue;
          switch ( stepWork.PredecessorType ) {
            case PredecessorType.FinishToStart:
              connection.ConnectorFormat.BeginConnect( stepWork.Shape, 4 );
              connection.ConnectorFormat.EndConnect( related.Shape, 2 );
              break;
            case PredecessorType.StartToStart:
              connection.ConnectorFormat.BeginConnect( related.Shape, 2 );
              connection.ConnectorFormat.EndConnect( stepWork.Shape, 2 );
              connection.Adjustments [ 1 ] = -3f;
              break;
            case PredecessorType.FinishToFinish:
              connection.ConnectorFormat.BeginConnect( related.Shape, 4 );
              connection.ConnectorFormat.EndConnect( stepWork.Shape, 4 );
              connection.Adjustments [ 1 ] = 3f;
              break;
            default:
              break;
          }
          connection.Line.Weight = 1f;
          // connection.Line.EndArrowheadStyle = MsoArrowheadStyle.msoArrowheadTriangle ;
        }
      }
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
      foreach ( var task in group ) {
        worksheet.Cells [ startRow, 2 ].Value = i;
        worksheet.Cells [ startRow, 3 ].Value = task.TaskName;
        worksheet.Cells [ startRow, 3 ].Style.Indent = 1;
        worksheet.Cells [ startRow, 7 ].Value = task.Duration;
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
    var k = 1.8f;
    var tasksByGroup = tasks.GroupBy( x => x.GroupTaskName );
    var i = 1;
    foreach ( var group in tasksByGroup ) {
      startRow++;
      foreach ( var task in group ) {
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
            Convert.ToSingle( rangeColumnRowStart.Left ) + offset,
            Convert.ToSingle( rangeColumnRowStart.Top ) + 7.5f, width, 5 );
          if ( rect != null ) {
            rect.Fill.ForeColor.RGB = ColorTranslator.ToOle( color );
            rect.Line.Visible = MsoTriState.msoFalse;
          }
        }
        startRow++;
      }
    }
  }

  //public static void AddLegend( this Worksheet xlWorkSheet, int startRow, int columnStart, Color color1, Color color2, Color color3 )
  //{
  //  var shapeDescribeSeason = xlWorkSheet.Shapes.AddShape(
  //      MsoAutoShapeType.msoShapeRectangle,
  //      Convert.ToSingle( xlWorkSheet.Range [ xlWorkSheet.Cells [ startRow, columnStart ], xlWorkSheet.Cells [ startRow, columnStart ] ].Left ),
  //      Convert.ToSingle( xlWorkSheet.Range [ xlWorkSheet.Cells [ startRow, columnStart ], xlWorkSheet.Cells [ startRow, columnStart ] ].Top ),
  //      15 * 1.808f, 13.5f );

  //  shapeDescribeSeason.Fill.ForeColor.RGB = ColorTranslator.ToOle( color1 );
  //  shapeDescribeSeason.Line.Visible = MsoTriState.msoFalse;
  //  xlWorkSheet.Cells [ startRow, columnStart - 4 ] = "冬期:";

  //  var shapeDescribeTask = xlWorkSheet.Shapes.AddShape(
  //    MsoAutoShapeType.msoShapeRectangle,
  //    Convert.ToSingle( xlWorkSheet.Range [ xlWorkSheet.Cells [ startRow + 1, columnStart ], xlWorkSheet.Cells [ startRow + 1, columnStart ] ].Left ),
  //    Convert.ToSingle( xlWorkSheet.Range [ xlWorkSheet.Cells [ startRow + 1, columnStart ], xlWorkSheet.Cells [ startRow + 1, columnStart ] ].Top ) + 5f, 15 * 1.808f, 5 );
  //  shapeDescribeTask.Fill.ForeColor.RGB = ColorTranslator.ToOle( color2 );
  //  shapeDescribeTask.Line.Visible = MsoTriState.msoFalse;
  //  xlWorkSheet.Cells [ startRow + 1, columnStart - 4 ] = "設置:";
  //  xlWorkSheet.Cells [ startRow + 1, columnStart - 8 ] = "凡例:";

  //  var shapeDescribeStepWork = xlWorkSheet.Shapes.AddShape( MsoAutoShapeType.msoShapeRectangle, xlWorkSheet.Range [ xlWorkSheet.Cells [ startRow + 2, columnStart ], xlWorkSheet.Cells [ startRow + 2, columnStart ] ].Left, xlWorkSheet.Range [ xlWorkSheet.Cells [ startRow + 2, columnStart ], xlWorkSheet.Cells [ startRow + 2, columnStart ] ].Top + 5f, 15 * 1.808f, 5 ) as Shape;
  //  shapeDescribeStepWork.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle( color3 );
  //  shapeDescribeStepWork.Line.Visible = MsoTriState.msoFalse;
  //  xlWorkSheet.Cells [ startRow + 2, columnStart - 4 ] = "撤去:";


  //}
}

public class ChartStepwork
{
  public long TaskId { get; set; }
  public long StepWorkId { get; set; }
  public float Start { get; set; }
  public float Duration { get; set; }
  //public long? RelatedProcessorStepWork { get; set; }
  //public float Lag { get; set; }
  public List<ChartPredecessor> Predecessors { get; set; } = new List<ChartPredecessor>();
  public int RowIndex { get; set; }
  public Shape? Shape { get; set; }
  public Color Color { get; set; } = Color.AliceBlue;
  public PredecessorType PredecessorType { get; set; }
}

public class ChartPredecessor
{
  public long RelatedProcessorStepWork { get; set; }
  public float Lag { get; set; }
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