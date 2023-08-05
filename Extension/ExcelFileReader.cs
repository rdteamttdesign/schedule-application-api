using ClosedXML.Excel;
using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Resources;
using SchedulingTool.Api.Resources.Extended;

namespace SchedulingTool.Api.Extension;

public static class ExcelFileReader
{
  public static List<object> ReadFromFile(
    Stream fileStream,
    ICollection<string> sheetNameList,
    ProjectSetting setting,
    long installColorId,
    long removalColorId,
    int maxDisplayOrder,
    out List<SheetImportMessage> messages )
  {
    messages = new List<SheetImportMessage>();
    using var workbook = new XLWorkbook( fileStream );
    var groupTasks = new List<object>();
    int index = maxDisplayOrder + 1;
    foreach ( var sheetName in sheetNameList ) {
      if ( !workbook.Worksheets.Contains( sheetName ) ) {
        messages.Add( new SheetImportMessage()
        {
          SheetName = sheetName,
          Status = SheetImportMessage.SheetImportStatus.NotFound
        } );
        continue;
      }
      var worksheet = workbook.Worksheet( sheetName );
      if ( !AssertFormat( worksheet ) ) {
        messages.Add( new SheetImportMessage()
        {
          SheetName = sheetName,
          Status = SheetImportMessage.SheetImportStatus.WrongFormat
        } );
        continue;
      }
      var usedRowCount = worksheet.RowsUsed().Count();
      var groupName = string.Empty;
      var groupId = string.Empty;
      GroupTaskResource? groupTask = null;
      for ( int i = 2; i < usedRowCount + 1; i++ ) {
        if ( worksheet.Row( i ).IsHidden ) {
          continue;
        }
        groupName = GetText( worksheet.Cell( i, 1 ).Value );
        if ( i == 2 && string.IsNullOrEmpty( groupName ) ) {
          groupName = "<blank>";
        }

        if ( !string.IsNullOrEmpty( groupName ) ) {
          groupId = Guid.NewGuid().ToString();
          groupTask = new GroupTaskResource()
          {
            Name = groupName,
            HideChildren = false,
            DisplayOrder = index,
            ColorId = installColorId,
            Type = "project",
            Id = groupId
          };
          groupTasks.Add( groupTask );
          index++;
          //index = 1;
        }
        var taskName = GetText( worksheet.Cell( i, 2 ).Value );
        //if ( string.IsNullOrEmpty( taskName ) ) {
        //  continue;
        //}
        var duration = GetFloat( worksheet.Cell( i, 4 ).Value );
        if ( duration == 0 ) {
          continue;
        }
        else if ( duration < 0 ) {
          duration = Math.Abs( duration );
        }
        var numberOfGroups = GetInt( worksheet.Cell( i, 5 ).Value );
        if ( numberOfGroups < 0 ) {
          numberOfGroups = Math.Abs( numberOfGroups );
        }
        var taskId = Guid.NewGuid().ToString();
        var task = new TaskResource()
        {
          Start = 0,
          Duration = duration,
          Name = GetText( worksheet.Cell( i, 2 ).Value ),
          Id = taskId,
          Type = "task",
          Detail = GetText( worksheet.Cell( i, 3 ).Value ),
          GroupId = groupId,
          DisplayOrder = index,
          Note = GetText( worksheet.Cell( i, 6 ).Value ),
          ColorId = installColorId,
          GroupsNumber = numberOfGroups
        };
        //if ( groupTask != null ) {
        //  groupTask.Tasks.Add( task );
        //  taskIndex++;
        //}
        int numberOfStepworks = 0;
        var totalStepworkPortion = 0d;
        for ( int j = 7; j < 17; j++ ) {
          var value = GetFloat( worksheet.Cell( i, j ).Value );
          if ( value == 0 ) {
            continue;
          }
          totalStepworkPortion += Math.Abs( value );
          numberOfStepworks++;
        }
        if ( numberOfStepworks == 0 || Math.Abs( totalStepworkPortion - 1 ) > 10e-7 ) {
          task.Predecessors = new List<PredecessorResource>();
        }
        else {
          task.Stepworks = new List<StepworkResource>();
          var offset = 0f;
          for ( int j = 7; j < 17; j++ ) {
            var percentStepwork = GetFloat( worksheet.Cell( i, j ).Value );
            if ( percentStepwork == 0 ) {
              continue;
            }
            var stepwork = new StepworkResource()
            {
              Start = offset,
              Duration = task.Duration.DaysToColumnWidth( setting.ColumnWidth ),
              PercentStepWork = Math.Abs( percentStepwork * 100 ),
              Name = Guid.NewGuid().ToString(),
              ParentTaskId = taskId,
              Id = Guid.NewGuid().ToString(),
              Type = "task",
              GroupId = groupId,
              DisplayOrder = index,
              Predecessors = new List<PredecessorResource>(),
              ColorId = percentStepwork > 0 ? installColorId : removalColorId,
              GroupNumbers = task.GroupsNumber
            };
            offset += stepwork.Duration;
            task.Stepworks.Add( stepwork );
          }
        }
        groupTasks.Add( task );
        index++;
      }
      messages.Add( new SheetImportMessage()
      {
        SheetName = sheetName,
        Status = SheetImportMessage.SheetImportStatus.Success
      } );
    }
    return groupTasks;
  }

  public static List<GroupTaskDetailResource> ReadFromFile(
   Stream fileStream,
   ICollection<string> sheetNameList,
   ProjectSetting setting,
   long installColorId,
   long removalColorId,
   out List<SheetImportMessage> messages )
  {
    messages = new List<SheetImportMessage>();
    using var workbook = new XLWorkbook( fileStream );
    var groupTasks = new List<GroupTaskDetailResource>();
    foreach ( var sheetName in sheetNameList ) {
      if ( !workbook.Worksheets.Contains( sheetName ) ) {
        messages.Add( new SheetImportMessage()
        {
          SheetName = sheetName,
          Status = SheetImportMessage.SheetImportStatus.NotFound
        } );
        continue;
      }
      var worksheet = workbook.Worksheet( sheetName );
      if ( !AssertFormat( worksheet ) ) {
        messages.Add( new SheetImportMessage()
        {
          SheetName = sheetName,
          Status = SheetImportMessage.SheetImportStatus.WrongFormat
        } );
        continue;
      }
      var usedRowCount = worksheet.RowsUsed().Count();
      var groupName = string.Empty;
      var groupId = string.Empty;
      GroupTaskDetailResource? groupTask = null;
      for ( int i = 2; i < usedRowCount + 1; i++ ) {
        if ( worksheet.Row( i ).IsHidden ) {
          continue;
        }
        groupName = GetText( worksheet.Cell( i, 1 ).Value );
        if ( i == 2 && string.IsNullOrEmpty( groupName ) ) {
          groupName = "<blank>";
        }

        if ( !string.IsNullOrEmpty( groupName ) ) {
          groupId = Guid.NewGuid().ToString();
          groupTask = new GroupTaskDetailResource()
          {
            GroupTaskName = groupName,
            //HideChildren = false,
            //DisplayOrder = index,
            //ColorId = installColorId,
            //Type = "project",
            //Id = groupId
            Tasks = new List<TaskDetailResource>()
          };
          groupTasks.Add( groupTask );
        }
        var taskName = GetText( worksheet.Cell( i, 2 ).Value );
        var duration = GetFloat( worksheet.Cell( i, 4 ).Value );
        if ( duration == 0 ) {
          continue;
        }
        else if ( duration < 0 ) {
          duration = Math.Abs( duration );
        }
        var numberOfGroups = GetInt( worksheet.Cell( i, 5 ).Value );
        if ( numberOfGroups < 0 ) {
          numberOfGroups = Math.Abs( numberOfGroups );
        }
        var taskId = Guid.NewGuid().ToString();
        var task = new TaskDetailResource()
        {
          //Start = 0,
          Duration = duration,
          TaskName = GetText( worksheet.Cell( i, 2 ).Value ),
          //Id = taskId,
          //Type = "task",
          Description = GetText( worksheet.Cell( i, 3 ).Value ),
          //GroupId = groupId,
          //DisplayOrder = index,
          Note = GetText( worksheet.Cell( i, 6 ).Value ),
          //ColorId = installColorId,
          NumberOfTeam = numberOfGroups,
          Stepworks = new List<StepworkDetailResource>()
        };
        groupTask?.Tasks.Add( task );
        int numberOfStepworks = 0;
        double totalStepworkPortion = 0;
        for ( int j = 7; j < 17; j++ ) {
          var value = GetFloat( worksheet.Cell( i, j ).Value );
          if ( value == 0 ) {
            continue;
          }
          totalStepworkPortion += Math.Abs( value );
          numberOfStepworks++;
        }
        if ( numberOfStepworks == 0 || Math.Abs( totalStepworkPortion - 1 ) > 10e-7 ) {
          task.Stepworks.Add( new StepworkDetailResource()
          {
            Portion = 1,
            ColorId = installColorId
          } );
        }
        else {
          //var offset = 0f;
          for ( int j = 7; j < 17; j++ ) {
            var percentStepwork = GetFloat( worksheet.Cell( i, j ).Value );
            if ( percentStepwork == 0 ) {
              continue;
            }
            var stepwork = new StepworkDetailResource()
            {
              //Start = offset,
              //Duration = task.Duration.DaysToColumnWidth( setting.ColumnWidth ),
              Portion = Math.Abs( percentStepwork ),
              //Name = Guid.NewGuid().ToString(),
              //ParentTaskId = taskId,
              //Id = Guid.NewGuid().ToString(),
              //Type = "task",
              //GroupId = groupId,
              //DisplayOrder = index,
              //Predecessors = new List<PredecessorResource>(),
              ColorId = percentStepwork > 0 ? installColorId : removalColorId,
              //GroupNumbers = task.GroupsNumber
            };
            //offset += stepwork.Duration;
            task.Stepworks.Add( stepwork );
          }
        }
        //groupTasks.Add( task );
        //index++;
      }
      messages.Add( new SheetImportMessage()
      {
        SheetName = sheetName,
        Status = SheetImportMessage.SheetImportStatus.Success
      } );
    }
    return groupTasks;
  }

  private static bool AssertFormat(IXLWorksheet sheet)
  {
    return GetText( sheet.Cell( 1, 1 ).Value ).Contains( "工種" )
      && GetText( sheet.Cell( 1, 2 ).Value ).Contains( "種別" )
      && GetText( sheet.Cell( 1, 3 ).Value ).Contains( "細目" )
      && GetText( sheet.Cell( 1, 4 ).Value ).Contains( "所要日数" )
      && GetText( sheet.Cell( 1, 5 ).Value ).Contains( "班数" )
      && GetText( sheet.Cell( 1, 6 ).Value ).Contains( "備考" );
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
        //worksheet.Cell( rowIndex, 6 ).Value = task.AmplifiedDuration;
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

  public class SheetImportMessage
  {
    public string SheetName { get; set; } = null!;
    public string Status { get; set; } = null!;

    public static class SheetImportStatus
    {
      public static string Success = "Success";
      public static string NotFound = "Not found";
      public static string WrongFormat = "Wrong format";
    }
  }
}
