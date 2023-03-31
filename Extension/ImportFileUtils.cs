using ClosedXML.Excel;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.projectdetail;

namespace SchedulingTool.Api.Extension;

public static class ImportFileUtils
{
  public static List<object> ReadFromFile( Stream fileStream, ICollection<string> sheetNameList )
  {
    using var workbook = new XLWorkbook( fileStream );
    var groupTasks = new List<object>();
    int index = 1;
    foreach ( var sheetName in sheetNameList ) {
      if ( !workbook.Worksheets.Contains( sheetName ) ) {
        continue;
      }
      var worksheet = workbook.Worksheet( sheetName );
      var usedRowCount = worksheet.RowsUsed().Count();
      var groupName = string.Empty;
      var groupId = string.Empty;
      GroupTaskResource? groupTask = null;
      for ( int i = 2; i < usedRowCount + 1; i++ ) {
        if ( worksheet.Row( i ).IsHidden ) {
          continue;
        }
        groupName = GetText( worksheet.Cell( i, 1 ).Value );

        if ( !string.IsNullOrEmpty( groupName ) ) {
          groupId = Guid.NewGuid().ToString();
          groupTask = new GroupTaskResource()
          {
            Name = groupName,
            HideChildren = false,
            DisplayOrder = index,
            ColorId = 1,
            Type = "project",
            Id = groupId
          };
          groupTasks.Add( groupTask );
          index++;
          //index = 1;
        }
        var taskName = GetText( worksheet.Cell( i, 2 ).Value );
        if ( string.IsNullOrEmpty( taskName ) ) {
          continue;
        }
        var taskId = Guid.NewGuid().ToString();
        var task = new TaskResource()
        {
          Start = 0,
          Duration = GetFloat( worksheet.Cell( i, 4 ).Value ),
          Name = GetText( worksheet.Cell( i, 2 ).Value ),
          Id = taskId,
          Type = "task",
          Detail = GetText( worksheet.Cell( i, 3 ).Value ),
          GroupId = groupId,
          DisplayOrder = index,
          Note = GetText( worksheet.Cell( i, 6 ).Value ),
          ColorId = 1,
          GroupsNumber = GetInt( worksheet.Cell( i, 5 ).Value )
        };
        //if ( groupTask != null ) {
        //  groupTask.Tasks.Add( task );
        //  taskIndex++;
        //}
        int numberOfStepworks = 0;
        for ( int j = 7; j < 17; j++ ) {
          if ( GetFloat( worksheet.Cell( i, j ).Value ) == 0 ) {
            break;
          }
          numberOfStepworks++;
        }
        if ( numberOfStepworks == 0 ) {
          task.Predecessors = new List<PredecessorResource>();
        }
        else {
          task.Stepworks = new List<StepworkResource>();
          for ( int j = 0; j < numberOfStepworks; j++ ) {
            var stepwork = new StepworkResource()
            {
              Start = 0,
              Duration = Convert.ToSingle( Math.Round( task.Duration * GetFloat( worksheet.Cell( i, j + 7 ).Value ), 2 ) ),
              PercentStepWork = GetFloat( worksheet.Cell( i, j + 7 ).Value ),
              Name = Guid.NewGuid().ToString(),
              ParentTaskId = taskId,
              Id = Guid.NewGuid().ToString(),
              Type = "task",
              GroupId = groupId,
              DisplayOrder = index,
              Predecessors = new List<PredecessorResource>()
              //ColorId = 
            };
            task.Stepworks.Add( stepwork );
          }
        }
        groupTasks.Add( task );
        index++;
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

  private static double GetDouble( XLCellValue cellValue )
  {
    return cellValue.IsNumber ? ( double ) cellValue.GetNumber() : 0;
  }
}
