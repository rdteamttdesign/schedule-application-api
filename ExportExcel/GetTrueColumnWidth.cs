using System ;

namespace ExcelSchedulingSample.Utils
{
  public static class GetTrueColumnWidth
  {
    public static double GetTrueColWidth( double width )
    {
      //DEDUCE WHAT THE COLUMN WIDTH WOULD REALLY GET SET TO
      double z = 1d ;
      if ( width >= ( 1 + 2 / 3 ) ) {
        z = Math.Round( ( Math.Round( 7 * ( width - 1 / 256 ), 0 ) - 5 ) / 7, 2 ) ;
      }
      else {
        z = Math.Round( ( Math.Round( 12 * ( width - 1 / 256 ), 0 ) - Math.Round( 5 * width, 0 ) ) / 12, 2 ) ;
      }

      //HOW FAR OFF? (WILL BE LESS THAN 1)
      double errorAmt = width - z ;

      //CALCULATE WHAT AMOUNT TO TACK ONTO THE ORIGINAL AMOUNT TO RESULT IN THE CLOSEST POSSIBLE SETTING 
      double adj = 0d ;
      if ( width >= ( 1 + 2 / 3 ) ) {
        adj = ( Math.Round( 7 * errorAmt - 7 / 256, 0 ) ) / 7 ;
      }
      else {
        adj = ( ( Math.Round( 12 * errorAmt - 12 / 256, 0 ) ) / 12 ) + ( 2 / 12 ) ;
      }

      //RETURN A SCALED-VALUE THAT SHOULD RESULT IN THE NEAREST POSSIBLE VALUE TO THE TRUE DESIRED SETTING
      if ( z > 0 ) {
        return width + adj ;
      }

      return 0d ;
    }
  }
}