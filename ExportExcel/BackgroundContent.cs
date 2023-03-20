using OfficeOpenXml ;
using OfficeOpenXml.Style ;

namespace ExcelSchedulingSample.Utils
{
  public static class BackgroundContent
  {
    public static void CreateBackGroundContent( this ExcelWorksheet ws, int numberContent, int numberTask, int roNumber, int coNumber, int numberMonth )
    {
      #region Set BackGroundContent

      ws.TabColor = System.Drawing.Color.Black ;
      // Create Range and Set Default
      ExcelRange range = ws.Cells[ roNumber, coNumber, numberTask + 3, numberContent ] ;

      ws.Column( 1 ).Width = GetTrueColumnWidth.GetTrueColWidth( 1.86 ) ;
      ws.Column( 2 ).Width = GetTrueColumnWidth.GetTrueColWidth( 3.57 ) ;
      ws.Column( 3 ).Width = GetTrueColumnWidth.GetTrueColWidth( 26.86 ) ;
      ws.Column( 4 ).Width = GetTrueColumnWidth.GetTrueColWidth( 19.29 ) ;
      ws.Column( 5 ).Width = GetTrueColumnWidth.GetTrueColWidth( 10.29 ) ;
      ws.Column( 6 ).Width = GetTrueColumnWidth.GetTrueColWidth( 7.71 ) ;
      ws.Column( 7 ).Width = GetTrueColumnWidth.GetTrueColWidth( 12 ) ;
      ws.Column( 8 ).Width = GetTrueColumnWidth.GetTrueColWidth( 9.8 ) ;
      ws.Column( 9 ).Width = GetTrueColumnWidth.GetTrueColWidth( 9.8 ) ;
      ws.Column( 10 ).Width = 1 / 0.58 ;
      ws.Row( 1 ).Height = 13.5 ;
      ws.Row( 2 ).Height = 30 ;
      ws.Row( 3 ).Height = 13.5 ;
      ws.Row( 4 ).Height = 13.5 ;
      ws.Row( 5 ).Height = 13.5 ;
      ws.Row( 6 ).Height = 6.5 ;
      ws.Cells[ 2, 2, 2, numberContent+1+numberMonth*6+2 ].Merge = true ;
      ws.Cells[ 2, 2, 2, numberContent + 1 + numberMonth * 6 + 1 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center ;
      ws.Cells[ 2, 2, 2, numberContent + 1 + numberMonth * 6 + 1 ].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom ;
      ws.Cells[ 2, 2  ].Value = "【21号橋 上部工工事工程表(その1)】" ;
      ws.Cells[ 2, 2 ].Style.Font.Size = 27 ;
      
      ws.Cells[ 5, 2  ].Value = "【○○○工法】";
      ws.Cells[ 5, 2,5,2 ].Style.Font.Size = 14 ;
      ws.Cells[ 5, 2 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left ;
      ws.Cells[ 5, 2 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center ;

      ws.Row( roNumber ).Height = 25 ;
      ws.Row( roNumber + 1 ).Height = 17 ;
      ws.Row( roNumber + 2 ).Height = 7 ;

      //Set Tile
      ws.Cells[ roNumber, 2, roNumber, 2 ].Value = "No" ;
      
      ws.Cells[ roNumber, 3, roNumber, 3 ].Value = "工       種" ;

      ws.Cells[ roNumber, 4, roNumber, 4 ].Value = "細       目" ;

      ws.Cells[ roNumber, 5, roNumber, 5 ].Value = "所要日数 (A)" ;

      ws.Cells[ roNumber, 6, roNumber, 6 ].Value = "台数班数" ;

      ws.Cells[ roNumber, 7, roNumber, 7 ].Value = "所要日数 (B) (Ax1.7/班数)" ;

      ws.Cells[ roNumber, 8, roNumber, 8 ].Value = "設置日数 (C)    (Bx0.6)" ;

      ws.Cells[ roNumber, 9, roNumber, 9 ].Value = "撤去日数 (D)    (Bx0.4)" ;
     
      for ( int i = 0 ; i < ( numberTask + 3 ) ; i++ ) {
        // Set Row 3 Down
        if ( i > 2 ) {
          ws.Row( roNumber + i ).Height = 19.5 ;
        }

        for ( int j = 0 ; j < numberContent ; j++ ) {
          // Merge Row1-3 
          if ( i < 3 ) 
          {
            range.Worksheet.Cells[ roNumber, coNumber + j, roNumber + 2, coNumber + j ].Merge = true ;
            //range.Worksheet.Cells[roNumber, coNumber + (j - 5), roNumber, coNumber + j].Value = (j + 1) / 6 + "月";
            range.Worksheet.Cells[ roNumber, coNumber + j, roNumber + 2, coNumber + j ].Style.Border.Left.Style = ExcelBorderStyle.Thin ;
            range.Worksheet.Cells[ roNumber, coNumber + j, roNumber + 2, coNumber + j ].Style.Border.Right.Style = ExcelBorderStyle.Thin ;
            range.Worksheet.Cells[ roNumber, coNumber + j, roNumber + 2, coNumber + j ].Style.Border.Top.Style = ExcelBorderStyle.Thin ;
            range.Worksheet.Cells[ roNumber, coNumber + j, roNumber + 2, coNumber + j ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin ;
            range.Worksheet.Cells[ roNumber, coNumber + j, roNumber + 2, coNumber + j ].Style.Font.Bold = true ;
            range.Worksheet.Cells[ roNumber, coNumber + j, roNumber + 2, coNumber + j ].Style.WrapText = true ;
            range.Worksheet.Cells[ roNumber, coNumber + j, roNumber + 2, coNumber + j ].Style.VerticalAlignment = ExcelVerticalAlignment.Center ;
            range.Worksheet.Cells[ roNumber, coNumber + j, roNumber + 2, coNumber + j ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center ;
          }
          // Set Style

          range[ roNumber + i, coNumber + j ].Style.Border.Right.Style = ExcelBorderStyle.Thin ;
          range[ roNumber + i, coNumber + j ].Style.Border.Left.Style = ExcelBorderStyle.Thin ;
          range[ roNumber + i, coNumber + j ].Style.Border.Top.Style = ExcelBorderStyle.Thin ;
          range[ roNumber + i, coNumber + j ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin ;

          if ( i > 2 ) 
          {
            range.Worksheet.Cells[ roNumber + i, coNumber +j ].Style.WrapText = true ;
            
          }
          if ( j==1 || j==2)
          {
            range[ roNumber + i, coNumber + j ].Style.VerticalAlignment = ExcelVerticalAlignment.Center ;
            range[ roNumber + i, coNumber + j ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left ;
          }
          
          if ( j==0 || j==4)
          {
            range[ roNumber + i, coNumber + j ].Style.VerticalAlignment = ExcelVerticalAlignment.Center ;
            range[ roNumber + i, coNumber + j ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center ;
          }
          
          if ( j==3 || j==5 || j==6 || j==7 )
          {
            range[ roNumber + i, coNumber + j ].Style.VerticalAlignment = ExcelVerticalAlignment.Center ;
            range[ roNumber + i, coNumber + j ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right ;
          }
        }
      }

      #endregion
    }
  }
}