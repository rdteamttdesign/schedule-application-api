using ExcelSchedulingSample.Class;
using Microsoft.Office.Core;
using OfficeOpenXml;
using Microsoft.Office.Interop.Excel;
using Shape = Microsoft.Office.Interop.Excel.Shape;
using Task = ExcelSchedulingSample.Class.Task;
using Range = Microsoft.Office.Interop.Excel.Range;

namespace ExcelSchedulingSample.Utils
{
  public static class LoadData
  {
    public static void LoadDataShow( this ExcelWorksheet ws, ref IEnumerable<Task> tasks, int numberMonth )
    {
      var startRow = 10 ;
      foreach ( var task in tasks ) {
        if ( ! task.IsTask ) {
          startRow++ ;
          continue ;
        }
        var startColumnInRow = 2 ;
        ws.Columns[ 10+numberMonth*6+1 ].AutoFit();
        ws.Cells[ startRow, 10+numberMonth*6+1 ].Value = task.Remarks ;
        var texts = new List<object>()
        {
          task.TaskId,
          task.TaskName ?? "",
          task.Description ?? "",
          task.NumberDay1,
          task.NumberTeam,
          task.NumberDay2,
          task.NumberDay3,
          task.NumberDay4,
        } ;
        foreach ( var text in texts ) {
          ws.Cells[ startRow, startColumnInRow ].Value = text ;
          if ( startColumnInRow==3 & task.TaskId !=null) {
            ws.Cells[ startRow, startColumnInRow ].Style.Indent = 1 ;
          }
          if (task.TaskId==null) {
            ws.Cells[ startRow, startColumnInRow ].Style.Font.Bold =true;
            ws.Cells[ startRow, startColumnInRow ].Style.WrapText = false ;
          }
          if ( startColumnInRow==5 & task.NumberDay1!=null ||startColumnInRow==7 & task.NumberDay2!=null ||startColumnInRow==8 & task.NumberDay3!=null ||startColumnInRow==9 & task.NumberDay4!=null  ) {
            ws.Cells[ startRow, startColumnInRow ].Value = text+ "日" ;
          }
          if ( startColumnInRow==6 & task.NumberTeam!=null ) {
            ws.Cells[ startRow, startColumnInRow ].Value = text+ "班" ;
          }
          if ( startColumnInRow==6 & task.TaskId !=null & task.NumberTeam==null||task.NumberTeam==0  ) {
            ws.Cells[ startRow, 6 ].Value = "-班" ;
          }
          startColumnInRow++ ;
        }
        task.RowIndex = startRow ;
        startRow++ ;
      }
    }

    public static double k = 1.80 ;
    public static void DrawChart( this Worksheet xlWorkSheet, IEnumerable<Task> tasks )
    {
      
      System.Drawing.Color color = System.Drawing.Color.Blue ;
      const int columnStart = 11 ;
      var listSteps = tasks.Where( t => t.IsTask ).SelectMany( x => x.StepWorks ).ToList() ;
      foreach ( var stepWork in listSteps ) {
        // CreateShape
        var offset = CalculateOffsetStepWork( stepWork, listSteps ) ;
        var rowStart = tasks.FirstOrDefault( x => x.StepWorks.Any( s => s.StepWorkId == stepWork.StepWorkId ) ).RowIndex ;
        // Get position cell top, left
        Range rangeColumnRowStart = xlWorkSheet.Range[ xlWorkSheet.Cells[ rowStart, columnStart ], xlWorkSheet.Cells[ rowStart, columnStart ] ] ;

        float width = (float )(stepWork.DurationDay * k) ;
        var rect = xlWorkSheet.Shapes.AddShape(
          MsoAutoShapeType.msoShapeRectangle,
          Convert.ToSingle( rangeColumnRowStart.Left ) + offset,
          Convert.ToSingle( rangeColumnRowStart.Top ) + 7.5f, width, 5f ) as Shape;
        if ( rect != null ) {
          rect.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle( stepWork.Color ?? color ) ;
          rect.Line.Visible= MsoTriState.msoFalse ;
          stepWork.Shape = rect ;
        }
      }

      foreach ( var stepWork in listSteps ) {
        if ( stepWork.PreSuffixDay == 0 ) {
          var related = listSteps.FirstOrDefault( x => x.StepWorkId == stepWork.RelatedProcessorStepWork ) ;
          var connection = xlWorkSheet.Shapes.AddConnector( MsoConnectorType.msoConnectorStraight, 1, 1, 1, 1 );
          connection.Line.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle( stepWork.Color ?? color ) ;
          
          if ( related?.Shape == null ) continue ;
          connection.ConnectorFormat.BeginConnect( related.Shape, 4 ) ;
          connection.ConnectorFormat.EndConnect( stepWork.Shape, 2 ) ;
          connection.Line.Weight = 1f ;
          connection.Line.BeginArrowheadLength = MsoArrowheadLength.msoArrowheadLengthMedium ;
          connection.Line.BeginArrowheadWidth = MsoArrowheadWidth.msoArrowheadNarrow ;
          
          // connection.Line.EndArrowheadStyle = MsoArrowheadStyle.msoArrowheadTriangle ;
        }
        else {
          var related = listSteps.FirstOrDefault( x => x.StepWorkId == stepWork.RelatedProcessorStepWork ) ;
          var connection = xlWorkSheet.Shapes.AddConnector( MsoConnectorType.msoConnectorElbow, 1, 1, 1, 1 ) ;
          connection.Line.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle( stepWork.Color ?? color ) ;
          
          if ( related?.Shape == null ) continue ;
          connection.ConnectorFormat.BeginConnect( related.Shape, 4 ) ;
          connection.ConnectorFormat.EndConnect( stepWork.Shape, 2 ) ;
          connection.Line.Weight = 1f ;
          // connection.Line.EndArrowheadStyle = MsoArrowheadStyle.msoArrowheadTriangle ;
        }
      }
    }
    private static float CalculateOffsetStepWork( StepWork stepWorksCheck, IEnumerable<StepWork> listStepWork )
    {
      float offsetValue = 0 ;
      if ( stepWorksCheck.RelatedProcessorStepWork == null ) return 0 ;

      var relatedStep = listStepWork.FirstOrDefault( s => s.StepWorkId == stepWorksCheck.RelatedProcessorStepWork ) ;
      if ( relatedStep == null ) {
        //MessageBox.Show( stepWorksCheck.StepWorkId + "-" + stepWorksCheck.RelatedProcessorStepWork ) ;
        return 0 ;
      }
      offsetValue += (float)( relatedStep.DurationDay * k ) ;
      offsetValue += CalculateOffsetStepWork( relatedStep, listStepWork ) ;
      if ( stepWorksCheck.PreSuffixDay < 0 ) {
        offsetValue = offsetValue - (float) Math.Abs(stepWorksCheck.PreSuffixDay * k )  ;
      }
      else {
        offsetValue = offsetValue + (float) Math.Abs(  stepWorksCheck.PreSuffixDay * k )  ;
      }

      return ( offsetValue ) ;
    }
  }
}