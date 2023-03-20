using OfficeOpenXml;
using OfficeOpenXml.Style;
using Color = System.Drawing.Color;

namespace ExcelSchedulingSample
{
  public static class BackgroundColor
  {
    public static void CreateBackGroundColor( this ExcelWorksheet ws, int dayStart, int dayFinish, int dayOfSeason, int rowStart, int columnStart, int numberTask, int numberMonth, Color color )
    {
      
      color = Color.Bisque ;
      int durationStart = dayFinish - dayStart ;
      int ortherDay = 12 * 30 - dayOfSeason ;

      if ( durationStart < dayOfSeason ) {
        ws.Cells[ rowStart + 3, columnStart, rowStart + 3 + numberTask - 1, columnStart + durationStart / 5 -1].Style.Fill.PatternType = ExcelFillStyle.Solid ;
        ws.Cells[ rowStart + 3, columnStart, rowStart + 3 + numberTask - 1, columnStart + durationStart / 5-1 ].Style.Fill.BackgroundColor.SetColor( color ) ;

        for ( int i = 0 ; i < (numberMonth * 30-durationStart)/5 ; i++ ) 
        {
          if ( i % (( ortherDay+dayOfSeason)/5) == 0 & i+dayOfSeason/5<numberMonth*6 ) {
            
            var column1 = columnStart  + dayFinish / 5 +ortherDay/5 + i;
            var column2 = columnStart-1 + dayFinish/  5+ortherDay/5+ i+dayOfSeason/5  ;
            var row1 = rowStart + 3 ;
            var row2 = rowStart + 3 + numberTask - 1 ;

            ws.Cells[ row1, column1, row2, column2 ].Style.Fill.PatternType = ExcelFillStyle.Solid ;
            ws.Cells[ row1, column1, row2, column2 ].Style.Fill.BackgroundColor.SetColor( color ) ;
          }
          if ( i % (( ortherDay+dayOfSeason)/5) == 0 & i+dayOfSeason/5>numberMonth*6 ) {
            
            var column1 = columnStart  + dayFinish / 5 +ortherDay/5 + i;
            var column2 = columnStart + numberMonth*6  ;
            var row1 = rowStart + 3 ;
            var row2 = rowStart + 3 + numberTask - 1 ;

            ws.Cells[ row1, column1, row2, column2 ].Style.Fill.PatternType = ExcelFillStyle.Solid ;
            ws.Cells[ row1, column1, row2, column2 ].Style.Fill.BackgroundColor.SetColor( color ) ;
          }
        }

      }
      else {
        for ( int i = 0 ; i < ( numberMonth * 30 - dayStart/5 ) / 5 ; i++ ) {
          if ( i % ( ( ortherDay + dayOfSeason ) / 5 ) == 0 & i + dayOfSeason / 5 < numberMonth * 6 ) {

            var column1 = columnStart + dayStart / 5 + i ;
            var column2 = columnStart - 1 + dayStart / 5  + dayOfSeason / 5+ i  ;
            var row1 = rowStart + 3 ;
            var row2 = rowStart + 3 + numberTask - 1 ;

            ws.Cells[ row1, column1, row2, column2 ].Style.Fill.PatternType = ExcelFillStyle.Solid ;
            ws.Cells[ row1, column1, row2, column2 ].Style.Fill.BackgroundColor.SetColor( color ) ;
          }

          if ( i % ( ( ortherDay + dayOfSeason ) / 5 ) == 0 & i + dayOfSeason / 5 > numberMonth * 6 ) {

            var column1 = columnStart + dayStart / 5  + i ;
            var column2 = columnStart + numberMonth * 6-1 ;
            var row1 = rowStart + 3 ;
            var row2 = rowStart + 3 + numberTask - 1 ;

            ws.Cells[ row1, column1, row2, column2 ].Style.Fill.PatternType = ExcelFillStyle.Solid ;
            ws.Cells[ row1, column1, row2, column2 ].Style.Fill.BackgroundColor.SetColor( color ) ;
          }
        }
      }
    }
  }
}
