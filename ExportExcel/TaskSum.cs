namespace ExcelSchedulingSample.Class
{
  public class TaskSum
  {
    public bool IsTask { get; set; } = true;
    public int RowIndex { get; set; } = 0;
    public int? TaskId { get; set; }
    public string TaskName { get; set; }
    public string Description { get; set; }
    // public double ? NumberDay1 { get ; set ; }
    public double? NumberDay2 { get; set; }
    public double? Duration { get; set; }
    public double? Offset { get; set; }
    // public double? NumberDay4 { get ; set ; }
    // public double? NumberTeam { get ; set ; }
    public string Remarks { get; set; }
  }

  public static class FakeData2
  {
    public static IEnumerable<TaskSum> FakeTaskSums()
    {
      var listOutSum = new List<TaskSum>();
      var task1 = new TaskSum()
      {
        TaskName = "架設用機械設備及び工具の供用日数",
      };
      listOutSum.Add( task1 );

      var task2 = new TaskSum()
      {
        TaskId = 1,
        TaskName = "ﾍﾞﾝﾄ基礎工",
        Description = null,
        NumberDay2 = 168.8,
        Duration = 168.8,
        Offset = 14.4,
        Remarks = null,
      };
      listOutSum.Add( task2 );
      var task3 = new TaskSum()
      {
        TaskId = 2,
        TaskName = "ﾍﾞﾝﾄ設備工",
        Description = null,
        NumberDay2 = 163.9,
        Duration = 163.9,
        Offset = 17.3,
        Remarks = null,
      };
      listOutSum.Add( task3 );
      var task4 = new TaskSum()
      {
        TaskId = 3,
        TaskName = "軌条設備工",
        Description = null,
        NumberDay2 = 132.3,
        Duration = 132.3,
        Offset = 218.8,
        Remarks = null,
      };
      listOutSum.Add( task4 );
      var task5 = new TaskSum()
      {
        TaskId = 4,
        TaskName = "軌条桁設備工",
        Description = null,
        NumberDay2 = 132.3,
        Duration = 132.3,
        Offset = 218.8,
        Remarks = null,
      };
      listOutSum.Add( task5 );
      var task6 = new TaskSum()
      {
        TaskId = 5,
        TaskName = "台車設備工",
        Description = null,
        NumberDay2 = 132.3,
        Duration = 132.3,
        Offset = 218.8,
        Remarks = null,
      };
      listOutSum.Add( task6 );
      var task7 = new TaskSum()
      {
        TaskId = 6,
        TaskName = "覆工板",
        Description = null,
        NumberDay2 = 132.3,
        Duration = 132.3,
        Offset = 218.8,
        Remarks = null,
      };
      listOutSum.Add( task7 );
      var task8 = new TaskSum()
      {
        TaskId = 7,
        TaskName = "ﾄﾞﾘﾌﾄﾋﾟﾝ",
        Description = null,
        NumberDay2 = 159.1,
        Duration = 159.1,
        Offset = 3.1,
        Remarks = null,
      };
      listOutSum.Add( task8 );
      listOutSum.Add( new TaskSum() { IsTask = false } );
      var task9 = new TaskSum()
      {
        TaskName = "足場工供用日数",
      };
      listOutSum.Add( task9 );
      var task10 = new TaskSum()
      {
        TaskId = 8,
        TaskName = "主体足場",
        Description = null,
        NumberDay2 = 16.1,
        Duration = 482.9,
        Offset = 107.40,
        Remarks = null,
      };
      listOutSum.Add( task10 );
      var task11 = new TaskSum()
      {
        TaskId = 9,
        TaskName = "中段足場",
        Description = null,
        NumberDay2 = 14.7,
        Duration = 439.5,
        Offset = 133.10,
        Remarks = null,
      };
      listOutSum.Add( task11 );
      var task12 = new TaskSum()
      {
        TaskId = 10,
        TaskName = "安全通路",
        Description = null,
        NumberDay2 = 4.4,
        Duration = 133.3,
        Offset = 74.3,
        Remarks = null,
      };
      listOutSum.Add( task12 );
      var task13 = new TaskSum()
      {
        TaskId = 11,
        TaskName = "部分作業床",
        Description = null,
        NumberDay2 = 4.2,
        Duration = 125.3,
        Offset = 79.1,
        Remarks = null,
      };
      listOutSum.Add( task13 );
      var task14 = new TaskSum()
      {
        TaskId = 12,
        TaskName = "朝顔",
        Description = null,
        NumberDay2 = 15.1,
        Duration = 453.4,
        Offset = 124.6,
        Remarks = null,
      };
      listOutSum.Add( task14 );
      var task15 = new TaskSum()
      {
        TaskId = 13,
        TaskName = "防護工",
        Description = "板張･両側朝顔",
        NumberDay2 = 14.7,
        Duration = 441.1,
        Offset = 132,
        Remarks = null,
      };
      listOutSum.Add( task15 );
      var task16 = new TaskSum()
      {
        TaskId = 14,
        TaskName = "登り桟橋工",
        Description = null,
        NumberDay2 = 19.8,
        Duration = 900,
        Offset = 0,
        Remarks = null,
      };
      listOutSum.Add( task16 );
      return listOutSum;
    }

  }
}
