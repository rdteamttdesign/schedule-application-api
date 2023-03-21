using OfficeOpenXml.Style;
using OfficeOpenXml;

namespace SchedulingTool.Api.ExportExcel;

public static class WorksheetFormater
{
  public static void CreateTitle( this ExcelWorksheet ws, int columnCount, int numberOfMonths )
  {
    ws.Cells [ 2, 2, 2, columnCount + 1 + numberOfMonths * 6 + 2 ].Merge = true;
    ws.Cells [ 2, 2, 2, columnCount + 1 + numberOfMonths * 6 + 1 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    ws.Cells [ 2, 2, 2, columnCount + 1 + numberOfMonths * 6 + 1 ].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
    ws.Cells [ 2, 2 ].Value = "【21号橋 上部工工事工程表(その1)】";
    ws.Cells [ 2, 2 ].Style.Font.Size = 27;

    ws.Cells [ 5, 2 ].Value = "【○○○工法】";
    ws.Cells [ 5, 2, 5, 2 ].Style.Font.Size = 14;
    ws.Cells [ 5, 2 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
    ws.Cells [ 5, 2 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
  }

  public static void FormatChartTable( this ExcelWorksheet ws, int startRow, int startColumn, int rowCount, int columnCount )
  {
    ws.TabColor = System.Drawing.Color.Black;
    // Create Range and Set Default
    ExcelRange range = ws.Cells [ startRow, startColumn, rowCount + 3, columnCount * 6 ];
    ws.Row( startRow ).Height = 25;
    ws.Row( startRow + 1 ).Height = 17;
    ws.Row( startRow + 2 ).Height = 7;

    //Set Column for Note
    range.Worksheet.Cells [ startRow + 1, startColumn + ( columnCount * 6 ), startRow + 2, startColumn + ( columnCount * 6 ) ].Merge = true;
    ws.Cells [ startRow, startColumn + columnCount * 6 ].Value = "備 　考";
    ws.Cells [ startRow, startColumn + columnCount * 6 ].Style.Font.Bold = true;
    for ( int x = 0; x < rowCount + 3; x++ ) {
      ws.Cells [ startRow + x, startColumn + columnCount * 6 ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
      ws.Cells [ startRow + x, startColumn + columnCount * 6 ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
      ws.Cells [ startRow + x, startColumn + columnCount * 6 ].Style.Border.Top.Style = ExcelBorderStyle.Thin;
      ws.Cells [ startRow + x, startColumn + columnCount * 6 ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
      ws.Cells [ startRow + x, startColumn + columnCount * 6 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      ws.Cells [ startRow + x, startColumn + columnCount * 6 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
    }
    ws.Cells [ startRow, startColumn + columnCount * 6 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    ws.Cells [ startRow, startColumn + columnCount * 6 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

    for ( int i = 0; i < ( rowCount + 3 ); i++ ) {
      // Set Row 3 Down
      if ( i > 2 ) {
        ws.Row( startRow + i ).Height = 19.5;
      }

      for ( int j = 0; j < ( columnCount * 6 ); j++ ) {
        // Set Column
        ws.Column( startColumn + j ).Width = 1 / 0.58;
        // Merge Row1 & Add Text
        if ( ( j + 1 ) % 6 == 0 & i == 0 ) {
          range.Worksheet.Cells [ startRow, startColumn + ( j - 5 ), startRow, startColumn + j ].Merge = true;
          range.Worksheet.Cells [ startRow, startColumn + ( j - 5 ), startRow, startColumn + j ].Value = ( j + 1 ) / 6 + "ヶ月目";
          range.Worksheet.Cells [ startRow, startColumn + ( j - 5 ), startRow, startColumn + j ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
          range.Worksheet.Cells [ startRow, startColumn + ( j - 5 ), startRow, startColumn + j ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
          range.Worksheet.Cells [ startRow, startColumn + ( j - 5 ), startRow, startColumn + j ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
          range.Worksheet.Cells [ startRow, startColumn + ( j - 5 ), startRow, startColumn + j ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
          range.Worksheet.Cells [ startRow, startColumn + ( j - 5 ), startRow, startColumn + j ].Style.Border.Top.Style = ExcelBorderStyle.Thin;
          range.Worksheet.Cells [ startRow, startColumn + ( j - 5 ), startRow, startColumn + j ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        // Merge Row2 and Set Style Row2
        if ( i == 1 ) {
          for ( int k = 1; k < columnCount * 6; k = k + 6 ) {
            range.Worksheet.Cells [ startRow + 1, startColumn + k, startRow + 1, startColumn + k + 1 ].Merge = true;
            range.Worksheet.Cells [ startRow + 1, startColumn + k + 2, startRow + 1, startColumn + k + 3 ].Merge = true;
            range.Worksheet.Cells [ startRow + 1, startColumn + k - 1 ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Worksheet.Cells [ startRow + 1, startColumn + k, startRow + 1, startColumn + k + 1 ].Value = 10;
            range.Worksheet.Cells [ startRow + 1, startColumn + k + 2, startRow + 1, startColumn + k + 3 ].Value = 20;
            range.Worksheet.Cells [ startRow + 1, startColumn + k, startRow + 1, startColumn + k + 1 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            range.Worksheet.Cells [ startRow + 1, startColumn + k, startRow + 1, startColumn + k + 1 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Worksheet.Cells [ startRow + 1, startColumn + k + 2, startRow + 1, startColumn + k + 3 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            range.Worksheet.Cells [ startRow + 1, startColumn + k + 2, startRow + 1, startColumn + k + 3 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
          }
          // Set style row3                        
        }

        if ( i == 2 ) {
          if ( j % 2 == 0 ) {
            range [ startRow + 2, startColumn + j ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
          }
        }

        // Set Style
        if ( ( j - 1 ) % 2 == 0 & i > 2 ) {
          range [ startRow + i, startColumn + j ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
          range [ startRow + i, startColumn + j ].Style.Border.Left.Style = ExcelBorderStyle.Hair;
          range [ startRow + i, startColumn + j ].Style.Border.Top.Style = ExcelBorderStyle.Thin;
          range [ startRow + i, startColumn + j ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        if ( ( j - 1 ) % 2 != 0 & i > 2 ) {
          range [ startRow + i, startColumn + j ].Style.Border.Top.Style = ExcelBorderStyle.Thin;
          range [ startRow + i, startColumn + j ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        if ( j == ( columnCount * 6 ) - 1 ) {
          range [ startRow + i, startColumn + j ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }

        if ( j == 0 ) {
          range [ startRow + i, startColumn + j ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        }
      }
    }
  }

  public static void FormatTaskTable( this ExcelWorksheet ws, int startRow, int startColumn, int rowCount, int columnCount )
  {
    #region Set BackGroundContent
    ws.TabColor = System.Drawing.Color.Black;
    // Create Range and Set Default
    ExcelRange range = ws.Cells [ startRow, startColumn, rowCount + 3, columnCount ];

    ws.Column( 1 ).Width = GetTrueColWidth( 1.86 );
    ws.Column( 2 ).Width = GetTrueColWidth( 3.57 );
    ws.Column( 3 ).Width = GetTrueColWidth( 26.86 );
    ws.Column( 4 ).Width = GetTrueColWidth( 19.29 );
    ws.Column( 5 ).Width = GetTrueColWidth( 10.29 );
    ws.Column( 6 ).Width = GetTrueColWidth( 7.71 );
    ws.Column( 7 ).Width = GetTrueColWidth( 12 );
    ws.Column( 8 ).Width = GetTrueColWidth( 9.8 );
    ws.Column( 9 ).Width = GetTrueColWidth( 9.8 );
    ws.Column( 10 ).Width = 1 / 0.58;
    ws.Row( 1 ).Height = 13.5;
    ws.Row( 2 ).Height = 30;
    ws.Row( 3 ).Height = 13.5;
    ws.Row( 4 ).Height = 13.5;
    ws.Row( 5 ).Height = 13.5;
    ws.Row( 6 ).Height = 6.5;
    
    ws.Row( startRow ).Height = 25;
    ws.Row( startRow + 1 ).Height = 17;
    ws.Row( startRow + 2 ).Height = 7;

    //Set Tile
    ws.Cells [ startRow, 2, startRow, 2 ].Value = "No";

    ws.Cells [ startRow, 3, startRow, 3 ].Value = "工       種";

    ws.Cells [ startRow, 4, startRow, 4 ].Value = "細       目";

    ws.Cells [ startRow, 5, startRow, 5 ].Value = "所要日数 (A)";

    ws.Cells [ startRow, 6, startRow, 6 ].Value = "台数班数";

    ws.Cells [ startRow, 7, startRow, 7 ].Value = "所要日数 (B) (Ax1.7/班数)";

    ws.Cells [ startRow, 8, startRow, 8 ].Value = "設置日数 (C)    (Bx0.6)";

    ws.Cells [ startRow, 9, startRow, 9 ].Value = "撤去日数 (D)    (Bx0.4)";

    for ( int i = 0; i < ( rowCount + 3 ); i++ ) {
      // Set Row 3 Down
      if ( i > 2 ) {
        ws.Row( startRow + i ).Height = 19.5;
      }

      for ( int j = 0; j < columnCount; j++ ) {
        // Merge Row1-3 
        if ( i < 3 ) {
          range.Worksheet.Cells [ startRow, startColumn + j, startRow + 2, startColumn + j ].Merge = true;
          //range.Worksheet.Cells[roNumber, coNumber + (j - 5), roNumber, coNumber + j].Value = (j + 1) / 6 + "月";
          range.Worksheet.Cells [ startRow, startColumn + j, startRow + 2, startColumn + j ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
          range.Worksheet.Cells [ startRow, startColumn + j, startRow + 2, startColumn + j ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
          range.Worksheet.Cells [ startRow, startColumn + j, startRow + 2, startColumn + j ].Style.Border.Top.Style = ExcelBorderStyle.Thin;
          range.Worksheet.Cells [ startRow, startColumn + j, startRow + 2, startColumn + j ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
          range.Worksheet.Cells [ startRow, startColumn + j, startRow + 2, startColumn + j ].Style.Font.Bold = true;
          range.Worksheet.Cells [ startRow, startColumn + j, startRow + 2, startColumn + j ].Style.WrapText = true;
          range.Worksheet.Cells [ startRow, startColumn + j, startRow + 2, startColumn + j ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
          range.Worksheet.Cells [ startRow, startColumn + j, startRow + 2, startColumn + j ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }
        // Set Style

        range [ startRow + i, startColumn + j ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        range [ startRow + i, startColumn + j ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        range [ startRow + i, startColumn + j ].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        range [ startRow + i, startColumn + j ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

        if ( i > 2 ) {
          range.Worksheet.Cells [ startRow + i, startColumn + j ].Style.WrapText = true;

        }
        if ( j == 1 || j == 2 ) {
          range [ startRow + i, startColumn + j ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
          range [ startRow + i, startColumn + j ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        }

        if ( j == 0 || j == 4 ) {
          range [ startRow + i, startColumn + j ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
          range [ startRow + i, startColumn + j ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        if ( j == 3 || j == 5 || j == 6 || j == 7 ) {
          range [ startRow + i, startColumn + j ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
          range [ startRow + i, startColumn + j ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        }
      }
    }
    #endregion
  }

  public static double GetTrueColWidth( double width )
  {
    //DEDUCE WHAT THE COLUMN WIDTH WOULD REALLY GET SET TO
    double z = 1d;
    if ( width >= ( 1 + 2 / 3 ) ) {
      z = Math.Round( ( Math.Round( 7 * ( width - 1 / 256 ), 0 ) - 5 ) / 7, 2 );
    }
    else {
      z = Math.Round( ( Math.Round( 12 * ( width - 1 / 256 ), 0 ) - Math.Round( 5 * width, 0 ) ) / 12, 2 );
    }

    //HOW FAR OFF? (WILL BE LESS THAN 1)
    double errorAmt = width - z;

    //CALCULATE WHAT AMOUNT TO TACK ONTO THE ORIGINAL AMOUNT TO RESULT IN THE CLOSEST POSSIBLE SETTING 
    double adj = 0d;
    if ( width >= ( 1 + 2 / 3 ) ) {
      adj = ( Math.Round( 7 * errorAmt - 7 / 256, 0 ) ) / 7;
    }
    else {
      adj = ( ( Math.Round( 12 * errorAmt - 12 / 256, 0 ) ) / 12 ) + ( 2 / 12 );
    }

    //RETURN A SCALED-VALUE THAT SHOULD RESULT IN THE NEAREST POSSIBLE VALUE TO THE TRUE DESIRED SETTING
    if ( z > 0 ) {
      return width + adj;
    }
    return 0d;
  }
}
