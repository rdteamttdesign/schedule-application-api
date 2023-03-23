﻿using Microsoft.Office.Core;
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
  public static void PopulateData( this ExcelWorksheet ws, IEnumerable<GroupTaskDetailResource> grouptasks, int startRow )
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

        ws.Cells [ startRow, 5 ].Value = $"{task.Duration}日";

        ws.Cells [ startRow, 6 ].Value = $"{task.NumberOfTeam}班";

        ws.Cells [ startRow, 7 ].Value = $"{task.AmplifiedDuration}日";

        if ( task.Stepworks.Count == 2 ) {
          ws.Cells [ startRow, 8 ].Value = $"{Math.Round( task.Stepworks.ElementAt( 0 ).Portion * task.AmplifiedDuration, 1 )}日";
          ws.Cells [ startRow, 9 ].Value = $"{Math.Round( task.Stepworks.ElementAt( 1 ).Portion * task.AmplifiedDuration, 1 )}日";
        }
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
      var offset = CalculateOffsetStepWork( stepwork, chartStepwork );
      // Get position cell top, left
      Range rangeColumnRowStart = xlWorkSheet.Range [ xlWorkSheet.Cells [ stepwork.RowIndex, startColumn ] ];

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
      if ( stepWork.Lag == 0 ) {
        var related = chartStepwork.FirstOrDefault( x => x.StepWorkId == stepWork.RelatedProcessorStepWork );
        var connection = xlWorkSheet.Shapes.AddConnector( MsoConnectorType.msoConnectorStraight, 1, 1, 1, 1 );
        connection.Line.ForeColor.RGB = ColorTranslator.ToOle( stepWork.Color );

        if ( related?.Shape == null )
          continue;
        connection.ConnectorFormat.BeginConnect( related.Shape, 4 );
        connection.ConnectorFormat.EndConnect( stepWork.Shape, 2 );
        connection.Line.Weight = 1f;
        connection.Line.BeginArrowheadLength = MsoArrowheadLength.msoArrowheadLengthMedium;
        connection.Line.BeginArrowheadWidth = MsoArrowheadWidth.msoArrowheadNarrow;

        // connection.Line.EndArrowheadStyle = MsoArrowheadStyle.msoArrowheadTriangle ;
      }
      else {
        var related = chartStepwork.FirstOrDefault( x => x.StepWorkId == stepWork.RelatedProcessorStepWork );
        var connection = xlWorkSheet.Shapes.AddConnector( MsoConnectorType.msoConnectorElbow, 1, 1, 1, 1 );
        connection.Line.ForeColor.RGB = ColorTranslator.ToOle( stepWork.Color );

        if ( related?.Shape == null )
          continue;
        connection.ConnectorFormat.BeginConnect( related.Shape, 4 );
        connection.ConnectorFormat.EndConnect( stepWork.Shape, 2 );
        connection.Line.Weight = 1f;
        // connection.Line.EndArrowheadStyle = MsoArrowheadStyle.msoArrowheadTriangle ;
      }
    }
  }

  private static float CalculateOffsetStepWork( ChartStepwork stepWorksCheck, IEnumerable<ChartStepwork> listStepWork )
  {
    double k = 1.80;
    float offsetValue = 0;
    if ( stepWorksCheck.RelatedProcessorStepWork == null )
      return 0;

    var relatedStep = listStepWork.FirstOrDefault( s => s.StepWorkId == stepWorksCheck.RelatedProcessorStepWork );
    if ( relatedStep == null ) {
      //MessageBox.Show( stepWorksCheck.StepWorkId + "-" + stepWorksCheck.RelatedProcessorStepWork );
      return 0;
    }
    offsetValue += ( float ) ( relatedStep.Duration * k );
    offsetValue += CalculateOffsetStepWork( relatedStep, listStepWork );

    return stepWorksCheck.Lag < 0
      ? offsetValue - ( float ) Math.Abs( stepWorksCheck.Lag * k ) 
      : offsetValue + ( float ) Math.Abs( stepWorksCheck.Lag * k );
  }
}

public class ChartStepwork
{
  public long StepWorkId { get; set; }
  public float Duration { get; set; }
  public long? RelatedProcessorStepWork { get; set; }
  public float Lag { get; set; }
  public int RowIndex { get; set; }
  public Shape? Shape { get; set; }
  public Color Color { get; set; }
  public PredecessorType PredecessorType { get; set; }
}

public enum PredecessorType
{
  FinishToStart = 1,
  StartToStart = 2,
  FinishToFinish = 3
}