using ExcelSchedulingSample.Class;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using Range = Microsoft.Office.Interop.Excel.Range;
using Shape = Microsoft.Office.Interop.Excel.Shape;

namespace ExcelSchedulingSample.Utils
{
  public static class LoadDataSum
  {
    public static void LoadDataSumShow( this ExcelWorksheet ws, ref IEnumerable<TaskSum> tasks, int numberMonth )
    {
      var startRow = 10;
      foreach ( var task in tasks ) {
        if ( !task.IsTask ) {
          startRow++;
          continue;
        }
        ws.Columns [ 10 + numberMonth * 6 + 1 ].AutoFit();
        ws.Cells [ startRow, 10 + numberMonth * 6 + 1 ].Value = task.Remarks;
        ws.Cells [ startRow, 2 ].Value = task.TaskId;
        if ( task.TaskId == null ) {
          ws.Cells [ startRow, 3 ].Style.Font.Bold = true;
          ws.Cells [ startRow, 3 ].Style.WrapText = false;
        }
        if ( task.TaskId != null ) {
          ws.Cells [ startRow, 3 ].Style.Indent = 1;
        }
        ws.Cells [ startRow, 3 ].Value = task.TaskName;
        ws.Cells [ startRow, 7 ].Value = task.NumberDay2;
        task.RowIndex = startRow;
        startRow++;
      }
    }

    public static void DrawChartSum( this Worksheet xlWorkSheet, IEnumerable<TaskSum> tasks )
    {
      System.Drawing.Color color = System.Drawing.Color.Violet;
      const int columnStart = 11;
      double k = 1.800;
      foreach ( var task in tasks ) {
        // CreateShape
        var offset = task.Offset ?? 0 * k;
        var rowStart = tasks.FirstOrDefault( x => x.TaskId == task.TaskId ).RowIndex;
        // Get position cell top, left
        Range rangeColumnRowStart = xlWorkSheet.Range [ xlWorkSheet.Cells [ rowStart, columnStart ], xlWorkSheet.Cells [ rowStart, columnStart ] ];
        if ( task.Duration > 0 ) {
          float width = ( float ) ( task.Duration * k );

          var rect = xlWorkSheet.Shapes.AddShape(
            MsoAutoShapeType.msoShapeRectangle,
            Convert.ToSingle(rangeColumnRowStart.Left) + Convert.ToSingle( offset ),
            Convert.ToSingle(rangeColumnRowStart.Top) + 7.5f, width, 5 ) as Shape;
          if ( rect != null ) {
            rect.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle( color );
            rect.Line.Visible = MsoTriState.msoFalse;
          }
        }
        else {
          continue;
        }
        rowStart++;
      }
    }
  }
}