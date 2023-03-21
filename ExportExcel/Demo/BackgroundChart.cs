using OfficeOpenXml ;
using OfficeOpenXml.Style ;

namespace ExcelSchedulingSample
{
  public static class BackgroundChart
  {
    //Set BackGround Utils at Cell[A1]
    public static void CreateBackgroundChart( this ExcelWorksheet ws, int numberMonth, int numberTask, int roNumber, int coNumber )
    {
      ws.TabColor = System.Drawing.Color.Black;
      // Create Range and Set Default
      ExcelRange range = ws.Cells [ roNumber, coNumber, numberTask + 3, ( numberMonth * 6 ) ];
      ws.Row( roNumber ).Height = 25;
      ws.Row( roNumber + 1 ).Height = 17;
      ws.Row( roNumber + 2 ).Height = 7;

      //Set Column for Note
      range.Worksheet.Cells [ roNumber + 1, coNumber + ( numberMonth * 6 ), roNumber + 2, coNumber + ( numberMonth * 6 ) ].Merge = true;
      ws.Cells [ roNumber, coNumber + numberMonth * 6 ].Value = "備 　考";
      ws.Cells [ roNumber, coNumber + numberMonth * 6 ].Style.Font.Bold = true;
      for ( int x = 0; x < numberTask + 3; x++ ) {
        ws.Cells [ roNumber + x, coNumber + numberMonth * 6 ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        ws.Cells [ roNumber + x, coNumber + numberMonth * 6 ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        ws.Cells [ roNumber + x, coNumber + numberMonth * 6 ].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        ws.Cells [ roNumber + x, coNumber + numberMonth * 6 ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        ws.Cells [ roNumber + x, coNumber + numberMonth * 6 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        ws.Cells [ roNumber + x, coNumber + numberMonth * 6 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
      }
      ws.Cells [ roNumber, coNumber + numberMonth * 6 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      ws.Cells [ roNumber, coNumber + numberMonth * 6 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

      for ( int i = 0; i < ( numberTask + 3 ); i++ ) {
        // Set Row 3 Down
        if ( i > 2 ) {
          ws.Row( roNumber + i ).Height = 19.5;
        }

        for ( int j = 0; j < ( numberMonth * 6 ); j++ ) {
          // Set Column
          ws.Column( coNumber + j ).Width = 1 / 0.58;
          // Merge Row1 & Add Text
          if ( ( j + 1 ) % 6 == 0 & i == 0 ) {
            range.Worksheet.Cells [ roNumber, coNumber + ( j - 5 ), roNumber, coNumber + j ].Merge = true;
            range.Worksheet.Cells [ roNumber, coNumber + ( j - 5 ), roNumber, coNumber + j ].Value = ( j + 1 ) / 6 + "ヶ月目";
            range.Worksheet.Cells [ roNumber, coNumber + ( j - 5 ), roNumber, coNumber + j ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            range.Worksheet.Cells [ roNumber, coNumber + ( j - 5 ), roNumber, coNumber + j ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Worksheet.Cells [ roNumber, coNumber + ( j - 5 ), roNumber, coNumber + j ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Worksheet.Cells [ roNumber, coNumber + ( j - 5 ), roNumber, coNumber + j ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            range.Worksheet.Cells [ roNumber, coNumber + ( j - 5 ), roNumber, coNumber + j ].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Worksheet.Cells [ roNumber, coNumber + ( j - 5 ), roNumber, coNumber + j ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
          }

          // Merge Row2 and Set Style Row2
          if ( i == 1 ) {
            for ( int k = 1; k < numberMonth * 6; k = k + 6 ) {
              range.Worksheet.Cells [ roNumber + 1, coNumber + k, roNumber + 1, coNumber + k + 1 ].Merge = true;
              range.Worksheet.Cells [ roNumber + 1, coNumber + k + 2, roNumber + 1, coNumber + k + 3 ].Merge = true;
              range.Worksheet.Cells [ roNumber + 1, coNumber + k - 1 ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
              range.Worksheet.Cells [ roNumber + 1, coNumber + k, roNumber + 1, coNumber + k + 1 ].Value = 10;
              range.Worksheet.Cells [ roNumber + 1, coNumber + k + 2, roNumber + 1, coNumber + k + 3 ].Value = 20;
              range.Worksheet.Cells [ roNumber + 1, coNumber + k, roNumber + 1, coNumber + k + 1 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
              range.Worksheet.Cells [ roNumber + 1, coNumber + k, roNumber + 1, coNumber + k + 1 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
              range.Worksheet.Cells [ roNumber + 1, coNumber + k + 2, roNumber + 1, coNumber + k + 3 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
              range.Worksheet.Cells [ roNumber + 1, coNumber + k + 2, roNumber + 1, coNumber + k + 3 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
            // Set style row3                        
          }

          if ( i == 2 ) {
            if ( j % 2 == 0 ) {
              range [ roNumber + 2, coNumber + j ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            }
          }

          // Set Style
          if ( ( j - 1 ) % 2 == 0 & i > 2 ) {
            range [ roNumber + i, coNumber + j ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            range [ roNumber + i, coNumber + j ].Style.Border.Left.Style = ExcelBorderStyle.Hair;
            range [ roNumber + i, coNumber + j ].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range [ roNumber + i, coNumber + j ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
          }

          if ( ( j - 1 ) % 2 != 0 & i > 2 ) {
            range [ roNumber + i, coNumber + j ].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range [ roNumber + i, coNumber + j ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
          }

          if ( j == ( numberMonth * 6 ) - 1 ) {
            range [ roNumber + i, coNumber + j ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
          }

          if ( j == 0 ) {
            range [ roNumber + i, coNumber + j ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
          }
        }
      }
    }
  }
}