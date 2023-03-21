using ExcelSchedulingSample.Class;
using ExcelSchedulingSample.Utils;
using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using System.Drawing;

namespace ExcelSchedulingSample;

public class ExcelSchedulingSample
{
  public static string? ExportToExcel()
  {
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    var excel = new ExcelPackage();
    var xlApp = new Application();
    string path = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, $"{Guid.NewGuid().ToString()}.xlsx" );
    try {
      var sheet1 = excel.Workbook.Worksheets.Add( "Sheet1" );
      var sheet2 = excel.Workbook.Worksheets.Add( "Sheet2" );
      sheet1.View.ShowGridLines = false;
      sheet2.View.ShowGridLines = false;

      sheet1.DeleteAllShapes();
      sheet2.DeleteAllShapes();
      const int rowNumber = 7;
      const int columNumber = 2;
      var numberContent = 8;
      var numberTask = 54;
      var numberMonth = 35;
      var columNumberChart = columNumber + numberContent + 1;

      // Set BackGround Sheet 1
      sheet1.CreateBackgroundChart( numberMonth, numberTask, rowNumber, columNumberChart );
      sheet1.CreateBackgroundContent( numberContent, numberTask, rowNumber, columNumber, numberMonth );
      sheet1.CreateBackgroundColor( 0, 120, 120, rowNumber, columNumberChart, numberTask, numberMonth, Color.Bisque );

      // Set BackGround Sheet 2
      var numberTaskSheet2 = 17;
      sheet2.CreateBackgroundChart( numberMonth, numberTaskSheet2, rowNumber, columNumberChart );
      sheet2.CreateBackgroundContent( numberContent, numberTaskSheet2, rowNumber, columNumber, numberMonth );
      sheet2.CreateBackgroundColor( 0, 120, 120, rowNumber, columNumberChart, numberTaskSheet2, numberMonth, Color.Bisque );

      var data = FakeData.FakeTasks();
      sheet1.LoadDataShow( ref data, numberMonth );

      var data2 = FakeData2.FakeTaskSums();
      sheet2.LoadDataSumShow( ref data2, numberMonth );

      excel.Save();
      if ( File.Exists( path ) )
        File.Delete( path );
      var fileStream = File.Create( path );
      fileStream.Close();
      File.WriteAllBytes( path, excel.GetAsByteArray() );
      excel.Dispose();
      var xlWorkBook = xlApp.Workbooks.Open( path );
      var xlWorkSheet = ( Worksheet ) xlWorkBook.Worksheets.get_Item( 1 );
      var xlWorkSheet2 = ( Worksheet ) xlWorkBook.Worksheets.get_Item( 2 );

      xlWorkSheet.DrawChart( data );
      xlWorkSheet2.DrawChartSum( data2 );

      xlWorkSheet.Descipes( rowNumber - 4, columNumberChart + numberMonth * 6, Color.Bisque, Color.Blue, Color.Red );
      xlWorkBook.Save();

      xlWorkBook.Close();
      xlApp.Quit();
      //Process.Start( path ) ;
      excel.Dispose();
      return path;
    }
    catch ( Exception e ) {
      excel.Dispose();
      xlApp.Quit();
      return null;
    }
  }
}
