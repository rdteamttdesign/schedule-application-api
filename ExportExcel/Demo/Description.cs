using Microsoft.Office.Core;
using Microsoft.Office.Interop.Excel;
using Shape = Microsoft.Office.Interop.Excel.Shape;

namespace ExcelSchedulingSample.Utils
{
  public static class Description
  {
    public static void Descipes( this Worksheet xlWorkSheet, int startRow, int columnStart, System.Drawing.Color color1, System.Drawing.Color color2, System.Drawing.Color color3 )
    {
      var shapeDescribeSeason = xlWorkSheet.Shapes.AddShape(
          MsoAutoShapeType.msoShapeRectangle,
          Convert.ToSingle( xlWorkSheet.Range [ xlWorkSheet.Cells [ startRow, columnStart ], xlWorkSheet.Cells [ startRow, columnStart ] ].Left ),
          Convert.ToSingle( xlWorkSheet.Range [ xlWorkSheet.Cells [ startRow, columnStart ], xlWorkSheet.Cells [ startRow, columnStart ] ].Top ),
          15 * 1.808f, 13.5f ) as Shape;

      shapeDescribeSeason.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle( color1 );
      shapeDescribeSeason.Line.Visible = MsoTriState.msoFalse;
      xlWorkSheet.Cells [ startRow, columnStart - 4 ] = "冬期:";

      var shapeDescribeTask = xlWorkSheet.Shapes.AddShape(
        MsoAutoShapeType.msoShapeRectangle,
        Convert.ToSingle(xlWorkSheet.Range[xlWorkSheet.Cells[startRow + 1, columnStart], xlWorkSheet.Cells[startRow + 1, columnStart]].Left),
        Convert.ToSingle(xlWorkSheet.Range[xlWorkSheet.Cells[startRow + 1, columnStart], xlWorkSheet.Cells[startRow + 1, columnStart]].Top) + 5f, 15 * 1.808f, 5 ) as Shape;
      shapeDescribeTask.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle( color2 );
      shapeDescribeTask.Line.Visible = MsoTriState.msoFalse;
      xlWorkSheet.Cells [ startRow + 1, columnStart - 4 ] = "設置:";
      xlWorkSheet.Cells [ startRow + 1, columnStart - 8 ] = "凡例:";

      var shapeDescribeStepWork = xlWorkSheet.Shapes.AddShape(
        MsoAutoShapeType.msoShapeRectangle,
        Convert.ToSingle(xlWorkSheet.Range[xlWorkSheet.Cells[startRow + 2, columnStart], xlWorkSheet.Cells[startRow + 2, columnStart]].Left),
        Convert.ToSingle(xlWorkSheet.Range[xlWorkSheet.Cells[startRow + 2, columnStart], xlWorkSheet.Cells[startRow + 2, columnStart]].Top) + 5f, 15 * 1.808f, 5 ) as Shape;
      shapeDescribeStepWork.Fill.ForeColor.RGB = System.Drawing.ColorTranslator.ToOle( color3 );
      shapeDescribeStepWork.Line.Visible = MsoTriState.msoFalse;
      xlWorkSheet.Cells [ startRow + 2, columnStart - 4 ] = "撤去:";
    }
  }
}