using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Drawing;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Domain.Models;

namespace SchedulingTool.Api.ExportExcel;

public static class WorksheetFormater
{
  private static int _tableStartRow = 7;
  private static int _tableStartColumn = 2;
  private static int _tableColumnCount = 8;
  private static int _chartStartColumn = 11;
  private static int _tableHeaderHeightSpan = 3;

  public static void CreateMainTableFrame( this ExcelWorksheet ws, ProjectResource resource, bool isMainView = true, View? view = null )
  {
    ws.Cells.Style.Font.Name = "MS Gothic";
    ws.View.ShowGridLines = false;
    ws.Drawings.Clear();

    var taskCount = isMainView
      ? resource.Grouptasks.SelectMany( x => x.Tasks ).Count() + resource.Grouptasks.Count + resource.Grouptasks.SelectMany( x => x.Tasks ).Count( x => x.Stepworks.Any( sw => sw.IsSubStepwork ) )
      : resource.ViewTasks [ view! ].Count + resource.ViewTasks [ view! ].GroupBy( x => x.GroupTaskId ).Count();

    if ( resource.Setting.IncludeYear ) {
      _tableHeaderHeightSpan = 4;
      ws.Row( _tableStartRow ).Height = 15;
      ws.Row( _tableStartRow + 1 ).Height = 17;
      ws.Row( _tableStartRow + 2 ).Height = 17;
      ws.Row( _tableStartRow + 3 ).Height = 7;
    }
    else {
      ws.Row( _tableStartRow ).Height = 27;
      ws.Row( _tableStartRow + 1 ).Height = 19;
      ws.Row( _tableStartRow + 2 ).Height = 8;
    }

    var numberOfMonths = resource.Backgrounds.Count / 3;
    ws.CreateTitle( resource.ProjectName, usedColumnSpan: 1 + _tableColumnCount + 1 + numberOfMonths * 6 + 1 );
    ws.CreateTaskListHeader( resource.Setting );

    if ( resource.Setting.IncludeYear )
      ws.CreateChartHeaderIncludeYear( resource.Backgrounds );
    else
      ws.CreateChartHeader( numberOfMonths );

    if ( taskCount == 0 ) {
      return;
    }

    ws.DrawTaskTableRow( taskCount );
    ws.DrawChartBackground( taskCount, numberOfMonths );
    ws.PaintChart( taskCount, resource.Backgrounds );
  }

  private static void CreateTitle( this ExcelWorksheet ws, string title, int usedColumnSpan )
  {
    var titleStartRow = 2;
    var titleStartColumn = 2;

    var cell = ws.Cells [ titleStartRow, titleStartColumn, titleStartRow, usedColumnSpan ];
    cell.Merge = true;
    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
    cell.Value = $"【{title}】";
    cell.Style.Font.Size = 27;
  }

  private static void CreateTaskListHeader( this ExcelWorksheet ws, ProjectSetting setting )
  {
    ws.TabColor = Color.Black;

    #region Set column width of task list
    ws.Column( 1 ).Width = GetTrueColumnWidth( 1.86 );
    ws.Column( 2 ).Width = GetTrueColumnWidth( 3.57 );
    ws.Column( 3 ).Width = GetTrueColumnWidth( 26.86 );
    ws.Column( 4 ).Width = GetTrueColumnWidth( 19.29 );
    ws.Column( 5 ).Width = GetTrueColumnWidth( 10.29 );
    ws.Column( 6 ).Width = GetTrueColumnWidth( 7.71 );
    ws.Column( 7 ).Width = GetTrueColumnWidth( 13.9 );
    ws.Column( 8 ).Width = GetTrueColumnWidth( 9.8 );
    ws.Column( 9 ).Width = GetTrueColumnWidth( 9.8 );
    ws.Column( 10 ).Width = 1 / 0.58;
    #endregion

    #region Set column name
    ws.Cells [ _tableStartRow, 2, _tableStartRow, 2 ].Value = "No";
    ws.Cells [ _tableStartRow, 3, _tableStartRow, 3 ].Value = "工       種";
    ws.Cells [ _tableStartRow, 4, _tableStartRow, 4 ].Value = "細       目";
    ws.Cells [ _tableStartRow, 5, _tableStartRow, 5 ].Value = "所要日数 (A)";
    ws.Cells [ _tableStartRow, 6, _tableStartRow, 6 ].Value = "台数班数";
    ws.Cells [ _tableStartRow, 7, _tableStartRow, 7 ].Value = $"所要日数(B)\n(Ax{setting.AmplifiedFactor}\n/班数)\n(日)";
    ws.Cells [ _tableStartRow, 8, _tableStartRow, 8 ].Value = $"設置日数 (C)\n(Bx{setting.AssemblyDurationRatio})";
    ws.Cells [ _tableStartRow, 9, _tableStartRow, 9 ].Value = $"撤去日数 (D)\n(Bx{setting.RemovalDurationRatio})";
    #endregion

    #region Merge cell and styling for header
    for ( int i = 0; i < _tableColumnCount; i++ ) {
      var cell = ws.Cells [ _tableStartRow, _tableStartColumn + i, _tableStartRow + _tableHeaderHeightSpan - 1, _tableStartColumn + i ];
      cell.Merge = true;
      cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
      cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
      cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
      cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
      cell.Style.Font.Bold = true;
      cell.Style.WrapText = true;
      cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    }
    #endregion
  }

  private static void CreateChartHeader( this ExcelWorksheet ws, int numberOfMonths )
  {
    ws.TabColor = Color.Black;

    #region Set unit timeline column width
    for ( int i = 0; i < numberOfMonths * 6; i++ ) {
      ws.Column( _chartStartColumn + i ).Width = 1 / 0.58;
    }
    #endregion

    #region Set month value
    for ( int i = 0; i < numberOfMonths; i++ ) {
      var range = ws.Cells [ _tableStartRow, _chartStartColumn + i * 6, _tableStartRow, _chartStartColumn + i * 6 + 5 ];
      range.Merge = true;
      range.Value = $"{i + 1}ヶ月目";
      range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
      range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
      range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
      range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
      range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    }
    #endregion

    #region Set block date value
    for ( int i = 0; i < numberOfMonths; i++ ) {
      var cellBlock10Days = ws.Cells [ _tableStartRow + 1, _chartStartColumn + i * 6 + 1, _tableStartRow + 1, _chartStartColumn + i * 6 + 2 ];
      cellBlock10Days.Merge = true;
      cellBlock10Days.Value = 10;
      cellBlock10Days.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      cellBlock10Days.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

      var cellBlock20Days = ws.Cells [ _tableStartRow + 1, _chartStartColumn + i * 6 + 3, _tableStartRow + 1, _chartStartColumn + i * 6 + 4 ];
      cellBlock20Days.Merge = true;
      cellBlock20Days.Value = 20;
      cellBlock20Days.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      cellBlock20Days.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

      ws.Cells [ _tableStartRow + 1, _chartStartColumn + i * 6 ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
    }
    ws.Cells [ _tableStartRow + 1, _chartStartColumn + numberOfMonths * 6 - 1 ].Style.Border.Right.Style = ExcelBorderStyle.Thin;

    for ( int i = 0; i < numberOfMonths * 6; i++ ) {
      if ( i % 6 == 0 ) {
        ws.Cells [ _tableStartRow + 2, _chartStartColumn + i ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
      }
      if ( i % 6 == 5 ) {
        ws.Cells [ _tableStartRow + 2, _chartStartColumn + i ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
      }
    }
    #endregion

    #region Set bottom header style
    // separate block day line
    for ( int i = 0; i < numberOfMonths * 6 - 1; i += 2 ) {
      ws.Cells [ _tableStartRow + 2, _chartStartColumn + i ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
      ws.Cells [ _tableStartRow + 2, _chartStartColumn + i + 1 ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
    }

    // bottom line
    for ( int i = 0; i < numberOfMonths * 6; i++ ) {
      ws.Cells [ _tableStartRow + 2, _chartStartColumn + i ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    }
    #endregion

    #region Create note column
    var noteRange = ws.Cells [
      _tableStartRow, _chartStartColumn + numberOfMonths * 6,
      _tableStartRow + _tableHeaderHeightSpan - 1, _chartStartColumn + numberOfMonths * 6 ];
    noteRange.Merge = true;
    noteRange.Value = "備 　考";
    noteRange.Style.Font.Bold = true;
    noteRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    noteRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    noteRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
    noteRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
    noteRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
    noteRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    #endregion
  }

  private static void CreateChartHeaderIncludeYear( this ExcelWorksheet ws, IList<ProjectBackgroundResource> backgrounds )
  {
    ws.TabColor = Color.Black;
    var numberOfMonths = backgrounds.Count / 3;

    #region Set unit timeline column width
    for ( int i = 0; i < backgrounds.Count * 2; i++ ) {
      ws.Column( _chartStartColumn + i ).Width = 1 / 0.58;
    }
    #endregion

    #region Set year month in header
    for ( int i = 0; i < numberOfMonths * 6; i++ ) {
      ws.Cells [ _tableStartRow, _chartStartColumn + i ].Style.Border.Top.Style = ExcelBorderStyle.Thin;
      if ( i % 6 == 0 ) {
        ws.Cells [ _tableStartRow, _chartStartColumn + i ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        ws.Cells [ _tableStartRow + 1, _chartStartColumn + i ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
      }
      if ( i % 6 == 5 ) {
        ws.Cells [ _tableStartRow, _chartStartColumn + i ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        ws.Cells [ _tableStartRow + 1, _chartStartColumn + i ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
      }
      ws.Cells [ _tableStartRow + 1, _chartStartColumn + i ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    }

    var year = int.MinValue;
    var month = int.MinValue;
    var monthCount = 0;
    foreach ( var resource in backgrounds ) {
      if ( resource.Year != year ) {
        var yearCell = ws.Cells [ _tableStartRow, _chartStartColumn + monthCount * 6, _tableStartRow, _chartStartColumn + monthCount * 6 + 5 ];
        yearCell.Merge = true;
        yearCell.Value = $"{resource.Year}年";
        yearCell.Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
        yearCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        year = resource.Year;
      }

      if ( resource.Month != month ) {
        var monthCell = ws.Cells [ _tableStartRow + 1, _chartStartColumn + monthCount * 6, _tableStartRow + 1, _chartStartColumn + monthCount * 6 + 5 ];
        monthCell.Merge = true;
        monthCell.Value = $"{resource.Month}月";
        monthCell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        monthCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        var yearCell = ws.Cells [ _tableStartRow, _chartStartColumn + monthCount * 6 ];
        yearCell.Style.Border.Left.Style = ExcelBorderStyle.Thin;

        month = resource.Month;
        monthCount++;
      }
    }
    #endregion

    #region Set block date value
    for ( int i = 0; i < numberOfMonths; i++ ) {

      var cellBlock10Days = ws.Cells [ _tableStartRow + 2, _chartStartColumn + i * 6 + 1, _tableStartRow + 2, _chartStartColumn + i * 6 + 2 ];
      cellBlock10Days.Merge = true;
      cellBlock10Days.Value = 10;
      cellBlock10Days.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      cellBlock10Days.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

      var cellBlock20Days = ws.Cells [ _tableStartRow + 2, _chartStartColumn + i * 6 + 3, _tableStartRow + 2, _chartStartColumn + i * 6 + 4 ];
      cellBlock20Days.Merge = true;
      cellBlock20Days.Value = 20;
      cellBlock20Days.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      cellBlock20Days.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

      ws.Cells [ _tableStartRow + 1, _chartStartColumn + i * 6 ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
    }

    for ( int i = 0; i < numberOfMonths * 6; i++ ) {
      if ( i % 6 == 0 ) {
        ws.Cells [ _tableStartRow + 2, _chartStartColumn + i ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
      }
      if ( i % 6 == 5 ) {
        ws.Cells [ _tableStartRow + 2, _chartStartColumn + i ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
      }
    }
    #endregion

    #region Set bottom header style
    // separate block day line
    for ( int i = 0; i < numberOfMonths * 6 - 1; i += 2 ) {
      ws.Cells [ _tableStartRow + 3, _chartStartColumn + i ].Style.Border.Left.Style = ExcelBorderStyle.Thin;
      ws.Cells [ _tableStartRow + 3, _chartStartColumn + i + 1 ].Style.Border.Right.Style = ExcelBorderStyle.Thin;
    }

    // bottom line
    for ( int i = 0; i < numberOfMonths * 6; i++ ) {
      ws.Cells [ _tableStartRow + 3, _chartStartColumn + i ].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    }
    #endregion

    #region Create note column
    var noteRange = ws.Cells [
      _tableStartRow, _chartStartColumn + numberOfMonths * 6,
      _tableStartRow + _tableHeaderHeightSpan - 1, _chartStartColumn + numberOfMonths * 6 ];
    noteRange.Merge = true;
    noteRange.Value = "備 　考";
    noteRange.Style.Font.Bold = true;
    noteRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    noteRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    noteRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
    noteRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
    noteRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
    noteRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    #endregion
  }

  private static void DrawTaskTableRow( this ExcelWorksheet ws, int taskCount )
  {
    ExcelRange range = ws.Cells [ _tableStartRow + _tableHeaderHeightSpan, 2, _tableStartRow + _tableHeaderHeightSpan - 1 + taskCount, 9 ];
    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    for ( int i = 0; i < taskCount; i++ ) {
      ws.Row( _tableStartRow + _tableHeaderHeightSpan + i ).Height = 19.5;

      ws.Cells [ _tableStartRow + _tableHeaderHeightSpan + i, 5 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      ws.Cells [ _tableStartRow + _tableHeaderHeightSpan + i, 5 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

      ws.Cells [ _tableStartRow + _tableHeaderHeightSpan + i, 6 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      ws.Cells [ _tableStartRow + _tableHeaderHeightSpan + i, 6 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

      ws.Cells [ _tableStartRow + _tableHeaderHeightSpan + i, 7 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      ws.Cells [ _tableStartRow + _tableHeaderHeightSpan + i, 7 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

      ws.Cells [ _tableStartRow + _tableHeaderHeightSpan + i, 8 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      ws.Cells [ _tableStartRow + _tableHeaderHeightSpan + i, 8 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

      ws.Cells [ _tableStartRow + _tableHeaderHeightSpan + i, 9 ].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      ws.Cells [ _tableStartRow + _tableHeaderHeightSpan + i, 9 ].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
    }
  }

  private static void DrawChartBackground( this ExcelWorksheet ws, int taskCount, int numberOfMonths )
  {
    for ( int rowIndex = 0; rowIndex < taskCount; rowIndex++ ) {
      for ( int currentUnitDate = 0; currentUnitDate <= numberOfMonths * 6 - 1; currentUnitDate++ ) {
        var cell = ws.Cells [ _tableStartRow + _tableHeaderHeightSpan + rowIndex, _chartStartColumn + currentUnitDate ];
        if ( currentUnitDate % 6 == 0 ) {
          cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
          cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
          cell.Style.Border.Right.Style = ExcelBorderStyle.Hair;
          cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }
        else if ( currentUnitDate % 6 == 5 ) {
          cell.Style.Border.Left.Style = ExcelBorderStyle.Hair;
          cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
          cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
          cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }
        else {
          cell.Style.Border.Left.Style = ExcelBorderStyle.Hair;
          cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
          cell.Style.Border.Right.Style = ExcelBorderStyle.Hair;
          cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }
      }
    }

    for ( int rowIndex = 0; rowIndex < taskCount; rowIndex++ ) {
      var cell = ws.Cells [ _tableStartRow + _tableHeaderHeightSpan + rowIndex, _chartStartColumn + numberOfMonths * 6 ];
      cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
      cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
      cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
      cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
      cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
      cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
    }
  }

  private static void PaintChart( this ExcelWorksheet ws, int numberOfTasks, IList<ProjectBackgroundResource> backgrounds )
  {
    for ( int i = 0; i < backgrounds.Count; i++ ) {
      if ( backgrounds [ i ].ColorId == null ) {
        continue;
      }
      var color = GetColor( backgrounds [ i ].ColorCode );
      var range = ws.Cells [
        _tableStartRow + _tableHeaderHeightSpan, _chartStartColumn + i * 2,
        _tableStartRow + _tableHeaderHeightSpan - 1 + numberOfTasks, _chartStartColumn + i * 2 + 1 ];
      range.Style.Fill.PatternType = ExcelFillStyle.Solid;
      range.Style.Fill.BackgroundColor.SetColor( color );
    }
  }

  private static double GetTrueColumnWidth( double width )
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

  public static Color GetColor( string code )
  {
    return ColorTranslator.FromHtml( code );
  }
}
