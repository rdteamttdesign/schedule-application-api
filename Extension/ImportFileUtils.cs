using ClosedXML.Excel;
using SchedulingTool.Api.Resources;

namespace SchedulingTool.Api.Extension;

public static class ImportFileUtils
{
  public static List<GroupTaskDetailResource> ReadFromFile( Stream fileStream, ICollection<string> sheetNameList )
  {
    using var workbook = new XLWorkbook( fileStream );
    var groupTasks = new List<GroupTaskDetailResource>();
    foreach ( var sheetName in sheetNameList ) {
      if ( !workbook.Worksheets.Contains( sheetName ) ) {
        continue;
      }
      var worksheet = workbook.Worksheet( sheetName );
      var usedRowCount = worksheet.RowsUsed().Count();
      var groupName = string.Empty;
      GroupTaskDetailResource? groupTask = null;
      for ( int i = 2; i < usedRowCount + 1; i++ ) {
        groupName = GetText( worksheet.Cell( i, 1 ).Value );
        if ( !string.IsNullOrEmpty( groupName ) ) {
          groupTask = new GroupTaskDetailResource()
          {
            GroupTaskName = groupName,
            Index = groupTasks.Count + 1,
            Tasks = new List<TaskDetailResource>()
          };
          groupTasks.Add( groupTask );
        }
        var task = new TaskDetailResource()
        {
          Index = i - 1,
          TaskName = GetText( worksheet.Cell( i, 2 ).Value ),
          Description = GetText( worksheet.Cell( i, 3 ).Value ),
          Duration = GetFloat( worksheet.Cell( i, 4 ).Value ),
          NumberOfTeam = GetInt( worksheet.Cell( i, 5 ).Value ),
          AmplifiedDuration = GetFloat( worksheet.Cell( i, 6 ).Value ),
          Note = GetText( worksheet.Cell( i, 7 ).Value ),
          Stepworks = new List<StepworkDetailResource>()
        };
        if ( groupTask != null ) {
          groupTask.Tasks.Add( task );
        }
        var numberOfStepworks = GetInt( worksheet.Cell( i, 8 ).Value );
        if ( numberOfStepworks == 0 ) {
          continue;
        }
        for ( int j = 0; j < numberOfStepworks; j++ ) {
          var stepwork = new StepworkDetailResource()
          {
            Index = j + 1,
            Portion = GetFloat( worksheet.Cell( i, j + 9 ).Value ),
            //ColorId = 
          };
          task.Stepworks.Add( stepwork );
        }
      }
    }
    return groupTasks;
  }

  public static byte[] WriteToFile( ICollection<GroupTaskDetailResource> groupTasks )
  {
    using var stream = new MemoryStream();
    using var workbook = new XLWorkbook();
    var worksheet = workbook.AddWorksheet();
    worksheet.Cell( 1, 1 ).Value = "工種";
    worksheet.Cell( 1, 2 ).Value = "種別";
    worksheet.Cell( 1, 3 ).Value = "細目";
    worksheet.Cell( 1, 4 ).Value = "所要日数";
    worksheet.Cell( 1, 5 ).Value = "班数";
    worksheet.Cell( 1, 6 ).Value = "所要日数\n1.7考慮";
    worksheet.Cell( 1, 7 ).Value = "備考";
    //worksheet.Cell( 1, 8 ).Value = "";
    int rowIndex = 2;
    for ( int i = 0; i < groupTasks.Count; i++ ) {
      var groupTask = groupTasks.ElementAt( i );
      worksheet.Cell( rowIndex, 1 ).Value = groupTask.GroupTaskName;
      for ( int j = 0; j < groupTask.Tasks.Count ; j++ ) {
        var task = groupTask.Tasks.ElementAt( j );
        worksheet.Cell( rowIndex, 2 ).Value = task.TaskName;
        worksheet.Cell( rowIndex, 3 ).Value = task.Description;
        worksheet.Cell( rowIndex, 4 ).Value = task.Duration;
        worksheet.Cell( rowIndex, 5 ).Value = task.NumberOfTeam;
        worksheet.Cell( rowIndex, 6 ).Value = task.AmplifiedDuration;
        worksheet.Cell( rowIndex, 7 ).Value = task.Note;
        worksheet.Cell( rowIndex, 8 ).Value = task.Stepworks.Count;
        for ( int k = 0; k < task.Stepworks.Count; k++ ) {
          worksheet.Cell( rowIndex, k + 9 ).Value = task.Stepworks.ElementAt(k).Portion;
        }
        rowIndex++;
      }
    }
    workbook.SaveAs( stream );
    return stream.ToArray();
  }

  private static string GetText( XLCellValue cellValue )
  {
    return cellValue.IsText ? cellValue.GetText() : cellValue.ToString();
  }

  private static int GetInt( XLCellValue cellValue )
  {
    return cellValue.IsNumber ? ( int ) cellValue.GetNumber() : 0;
  }

  private static float GetFloat( XLCellValue cellValue )
  {
    return cellValue.IsNumber ? ( float ) cellValue.GetNumber() : 0;
  }
}
