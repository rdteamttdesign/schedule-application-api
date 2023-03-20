
using System.Collections.Generic ;
using System.Drawing ;
using Microsoft.Office.Interop.Excel ;

namespace ExcelSchedulingSample.Class
{
  public class Task
  {
    public bool IsTask { get; set; } = true;
    public int RowIndex { get; set; } = 0;
    public int? TaskId { get; set; }
    public string TaskName { get; set; }
    public string Description { get; set; }
    public double? NumberDay1 { get; set; }
    public double? NumberDay2 { get; set; }
    public double? NumberDay3 { get; set; }
    public double? NumberDay4 { get; set; }
    public double? NumberTeam { get; set; }
    public string Remarks { get; set; }

    public List<StepWork> StepWorks { get; set; } = new List<StepWork>();
  }

  public class StepWork
  {
    public int StepWorkId { get; set; }
    public double DurationDay { get; set; }
    public int? RelatedProcessorStepWork { get; set; }

    public double PreSuffixDay { get; set; }
    public Color? Color { get; set; }
    public Shape Shape { get; set; }
  }

  public static class FakeData
  {
    public static IEnumerable<Task> FakeTasks()
    {
      var listOut = new List<Task>();

      var task1 = new Task()
      {
        TaskName = "ｸﾚｰﾝﾍﾞﾝﾄ架設",
      };
      listOut.Add( task1 );

      var task2 = new Task()
      {
        TaskId = 1,
        TaskName = "ﾍﾞﾝﾄ基礎工",
        Description = null,
        NumberDay1 = 2.9,
        NumberTeam = 1,
        NumberDay2 = 4.9,
        NumberDay3 = 2.9,
        NumberDay4 = 2,
        Remarks = null,
        StepWorks = new List<StepWork>() {
          new StepWork() { StepWorkId = 1, DurationDay = 2.9, RelatedProcessorStepWork = 15, PreSuffixDay = 0 },
          new StepWork() { StepWorkId = 2, DurationDay = 2, RelatedProcessorStepWork = 4, PreSuffixDay = 0 ,Color = Color.Red}
        }
      };
      listOut.Add( task2 );


      //other tasks
      var task3 = new Task()
      {
        TaskId = 2,
        TaskName = "ﾍﾞﾝﾄ設備工",
        Description = null,
        NumberDay1 = 28.0,
        NumberTeam = 1,
        NumberDay2 = 47.6,
        NumberDay3 = 28.6,
        NumberDay4 = 19,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 3, DurationDay = 28.6, RelatedProcessorStepWork = 1, },
          new StepWork() { StepWorkId = 4, DurationDay = 19, RelatedProcessorStepWork = 16, PreSuffixDay = 0 ,Color = Color.Red}
        }
      };
      listOut.Add( task3 );

      //other tasks
      var task4 = new Task()
      {
        TaskId = 3,
        TaskName = "軌条設備工",
        Description = null,
        NumberDay1 = 4.3,
        NumberTeam = 1,
        NumberDay2 = 7.3,
        NumberDay3 = 4.4,
        NumberDay4 = 2.9,
        Remarks = null,
        StepWorks = new List<StepWork>() {
          new StepWork() { StepWorkId = 5, DurationDay = 4.4, RelatedProcessorStepWork = 11, },
          new StepWork() { StepWorkId = 6, DurationDay = 2.9, RelatedProcessorStepWork = 10, Color = Color.Red},
           }
      };
      listOut.Add( task4 );


      var task5 = new Task()
      {
        TaskId = 4,
        TaskName = "軌条桁設備工",
        Description = null,
        NumberDay1 = 15.9,
        NumberTeam = 1,
        NumberDay2 = 27,
        NumberDay3 = 16.2,
        NumberDay4 = 10.8,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 7, DurationDay = 16.2, RelatedProcessorStepWork = 37, },
          new StepWork() { StepWorkId = 8, DurationDay = 10.8, RelatedProcessorStepWork = 12,Color = Color.Red }
        }
      };
      listOut.Add( task5 );

      var task6 = new Task()
      {
        TaskId = 5,
        TaskName = "台車設備工",
        Description = null,
        NumberDay1 = 2.5,
        NumberTeam = 1,
        NumberDay2 = 4.3,
        NumberDay3 = 2.6,
        NumberDay4 = 1.7,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 9, DurationDay = 2.6, RelatedProcessorStepWork = 5, },
          new StepWork() { StepWorkId = 10, DurationDay = 1.7, RelatedProcessorStepWork = 18,Color = Color.Red }
        }
      };
      listOut.Add( task6 );
      var task7 = new Task()
      {
        TaskId = 6,
        TaskName = "覆工板",
        Description = null,
        NumberDay1 = 19.8,
        NumberTeam = 1,
        NumberDay2 = 33.7,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "設置",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 11, DurationDay = 33.7, RelatedProcessorStepWork = 7, }, }
      };
      listOut.Add( task7 );
      var task8 = new Task()
      {
        TaskId = 7,
        TaskName = "覆工板",
        Description = null,
        NumberDay1 = 12,
        NumberTeam = 1,
        NumberDay2 = 20.4,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "撤去",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 12, DurationDay = 20.4, RelatedProcessorStepWork = 6, Color = Color.Red }, }
      };
      listOut.Add( task8 );
      var task9 = new Task()
      {
        TaskId = 8,
        TaskName = "地組工",
        Description = "少数I桁",
        NumberDay1 = 16.7,
        NumberTeam = 1,
        NumberDay2 = 28.4,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>() { new StepWork()
        { StepWorkId = 13, DurationDay = 28.4, RelatedProcessorStepWork = 3,PreSuffixDay=0 }, }
      };
      listOut.Add( task9 );
      var task10 = new Task()
      {
        TaskId = 9,
        TaskName = "桁架設工",
        Description = "少数I桁",
        NumberDay1 = 15.1,
        NumberTeam = 1,
        NumberDay2 = 25.7,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 14, DurationDay = 25.7, RelatedProcessorStepWork = 25, }, }
      };
      listOut.Add( task10 );

      var task11 = new Task()
      {
        TaskId = 10,
        TaskName = "支承据付工",
        Description = null,
        NumberDay1 = 13.3,
        NumberTeam = 2,
        NumberDay2 = 11.3,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "W=4.2t/基",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 15, DurationDay = 11.3, RelatedProcessorStepWork = 31, PreSuffixDay = 0 }, }
      };
      listOut.Add( task11 );
      var task12 = new Task()
      {
        TaskId = 11,
        TaskName = "高力ﾎﾞﾙﾄ本締工",
        Description = null,
        NumberDay1 = 25.5,
        NumberTeam = 2,
        NumberDay2 = 21.7,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 16, DurationDay = 21.7, RelatedProcessorStepWork = 21, }, }
      };
      listOut.Add( task12 );
      var task13 = new Task()
      {
        TaskId = 12,
        TaskName = "ﾋﾟﾝﾃｰﾙ処理工",
        Description = null,
        NumberDay1 = 11.7,
        NumberTeam = 2,
        NumberDay2 = 9.9,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 17, DurationDay = 9.9, RelatedProcessorStepWork = 16, }, }
      };
      listOut.Add( task13 );
      var task14 = new Task()
      {
        TaskId = 13,
        TaskName = "合成床版工",
        Description = "合成床版架設工",
        NumberDay1 = 54.1,
        NumberTeam = 1,
        NumberDay2 = 92,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 18, DurationDay = 39.6, RelatedProcessorStepWork = 9, },
          new StepWork() { StepWorkId = 182, DurationDay = 52.4, RelatedProcessorStepWork = 8, }
        }
      };
      listOut.Add( task14 );
      var task15 = new Task()
      {
        TaskId = 14,
        TaskName = "足場工",
        Description = "主体足場",
        NumberDay1 = 20.2,
        NumberTeam = 2,
        NumberDay2 = 17.2,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "設置",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 19, DurationDay = 17.2, RelatedProcessorStepWork = 14, }, }
      };
      listOut.Add( task15 );
      var task16 = new Task()
      {
        TaskId = 15,
        TaskName = "足場工",
        Description = "主体足場",
        NumberDay1 = 14.5,
        NumberTeam = 1,
        NumberDay2 = 12.3,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "撤去",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 20, DurationDay = 12.3, RelatedProcessorStepWork = 28, Color = Color.Red }, }
      };
      listOut.Add( task16 );
      var task17 = new Task()
      {
        TaskId = 16,
        TaskName = "足場工",
        Description = "中段足場",
        NumberDay1 = 8.7,
        NumberTeam = 2,
        NumberDay2 = 7.4,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "設置",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 21, DurationDay = 7.4, RelatedProcessorStepWork = 29, }, }
      };
      listOut.Add( task17 );
      var task18 = new Task()
      {
        TaskId = 17,
        TaskName = "足場工",
        Description = "中段足場",
        NumberDay1 = 6.5,
        NumberTeam = 2,
        NumberDay2 = 5.5,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "撤去",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 22, DurationDay = 5.5, RelatedProcessorStepWork = 49, Color = Color.Red }, }
      };
      listOut.Add( task18 );
      var task19 = new Task()
      {
        TaskId = 18,
        TaskName = "足場工",
        Description = "安全通路",
        NumberDay1 = 9.4,
        NumberTeam = 2,
        NumberDay2 = 8,
        NumberDay3 = 4.8,
        NumberDay4 = 3.2,
        Remarks = "設置･撤去",
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 23, DurationDay = 4.8, RelatedProcessorStepWork = 13 },
          new StepWork() { StepWorkId = 24, DurationDay = 3.2, RelatedProcessorStepWork = 26, Color = Color.Red}
        }
      };
      listOut.Add( task19 );
      var task20 = new Task()
      {
        TaskId = 19,
        TaskName = "足場工",
        Description = "部分作業床",
        NumberDay1 = 5.1,
        NumberTeam = 2,
        NumberDay2 = 4.3,
        NumberDay3 = 2.6,
        NumberDay4 = 1.7,
        Remarks = "設置･撤去",
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 25, DurationDay = 2.6, RelatedProcessorStepWork = 23, },
          new StepWork() { StepWorkId = 26, DurationDay = 1.7, RelatedProcessorStepWork = 33, Color = Color.Red}
        }
      };
      listOut.Add( task20 );
      var task21 = new Task()
      {
        TaskId = 20,
        TaskName = "足場工",
        Description = "朝顔",
        NumberDay1 = 8.7,
        NumberTeam = 2,
        NumberDay2 = 7.4,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "設置",
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 27, DurationDay = 7.4, RelatedProcessorStepWork = 19, }

        }
      };
      listOut.Add( task21 );
      var task22 = new Task()
      {
        TaskId = 21,
        TaskName = "足場工",
        Description = "朝顔",
        NumberDay1 = 5.8,
        NumberTeam = 2,
        NumberDay2 = 4.9,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "撤去",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 28, DurationDay = 4.9, RelatedProcessorStepWork = 30, Color = Color.Red }, }
      };
      listOut.Add( task22 );
      var task23 = new Task()
      {
        TaskId = 22,
        TaskName = "防護工",
        Description = "板張防護工",
        NumberDay1 = 1.3,
        NumberTeam = 2,
        NumberDay2 = 1.1,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "設置",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 29, DurationDay = 1.1, RelatedProcessorStepWork = 27, }, }
      };
      listOut.Add( task23 );
      var task24 = new Task()
      {
        TaskId = 23,
        TaskName = "防護工",
        Description = "板張防護工",
        NumberDay1 = 0.6,
        NumberTeam = 2,
        NumberDay2 = 0.5,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "撤去",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 30, DurationDay = 0.5, RelatedProcessorStepWork = 22, Color = Color.Red }, }
      };
      listOut.Add( task24 );
      var task25 = new Task()
      {
        TaskId = 24,
        TaskName = "登り桟橋工",
        Description = "手摺先行工法有り",
        NumberDay1 = 3.7,
        NumberTeam = 2,
        NumberDay2 = 3.1,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "設置",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 31, DurationDay = 3.1, RelatedProcessorStepWork = null, }, }
      };
      listOut.Add( task25 );
      var task26 = new Task()
      {
        TaskId = 25,
        TaskName = "登り桟橋工",
        Description = "手摺先行工法有り",
        NumberDay1 = 2.8,
        NumberTeam = 2,
        NumberDay2 = 2.4,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "撤去",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 32, DurationDay = 2.4, RelatedProcessorStepWork = 20, }, }
      };
      listOut.Add( task26 );
      var task27 = new Task()
      {
        TaskId = 26,
        TaskName = "現場塗装工",
        Description = null,
        NumberDay1 = 36,
        NumberTeam = 2,
        NumberDay2 = 30.6,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 33, DurationDay = 30.6, RelatedProcessorStepWork = 17, }, }
      };
      listOut.Add( task27 );
      var task28 = new Task()
      {

        TaskName = "巻立てｺﾝｸﾘｰﾄ工",

      };
      listOut.Add( task28 );
      var task29 = new Task()
      {
        TaskId = 27,
        TaskName = "ｺﾝｸﾘｰﾄ工",
        Description = "ｺﾝｸﾘｰﾄ工",
        NumberDay1 = 0.7,
        NumberTeam = 1,
        NumberDay2 = 1.2,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 34, DurationDay = 1.2, RelatedProcessorStepWork = 38, }, }
      };
      listOut.Add( task29 );
      var task30 = new Task()
      {
        TaskId = 28,
        TaskName = null,
        Description = "ｺﾝｸﾘｰﾄ養生工",
        NumberDay1 = 5,
        NumberTeam = 0,
        NumberDay2 = 5,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "稼働率100%",
        StepWorks = new List<StepWork>() { new StepWork() { StepWorkId = 35, DurationDay = 5, RelatedProcessorStepWork = 34, }, }
      };
      listOut.Add( task30 );
      var task31 = new Task()
      {
        TaskId = 29,
        TaskName = "型枠工",
        Description = null,
        NumberDay1 = 4.3,
        NumberTeam = 2,
        NumberDay2 = 3.7,
        NumberDay3 = 2.2,
        NumberDay4 = 1.5,
        Remarks = "設置･撤去",
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 36, DurationDay = 2.2, RelatedProcessorStepWork = 24, },
          new StepWork() { StepWorkId = 37, DurationDay = 1.5, RelatedProcessorStepWork = 35,Color = Color.Red}
        }
      };
      listOut.Add( task31 );
      var task32 = new Task()
      {
        TaskId = 30,
        TaskName = "鉄筋工",
        Description = null,
        NumberDay1 = 1.5,
        NumberTeam = 2,
        NumberDay2 = 1.3,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 38, DurationDay = 1.3, RelatedProcessorStepWork = 36, },

        }
      };
      listOut.Add( task32 );

      var task33 = new Task()
      {

        TaskName = "床板工",

      };
      listOut.Add( task33 );

      var task34 = new Task()
      {
        TaskId = 31,
        TaskName = "鉄筋工",
        Description = null,
        NumberDay1 = 24.4,
        NumberTeam = 2,
        NumberDay2 = 20.7,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 39, DurationDay = 20.7, RelatedProcessorStepWork = 182, },
        }
      };
      listOut.Add( task34 );
      var task35 = new Task()
      {
        TaskId = 32,
        TaskName = "地覆・壁高欄部型枠工",
        Description = null,
        NumberDay1 = 38.1,
        NumberTeam = 2,
        NumberDay2 = 32.4,
        NumberDay3 = 19.4,
        NumberDay4 = 13,
        Remarks = "設置･撤去",
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 40, DurationDay = 19.4, RelatedProcessorStepWork = 42, },
          new StepWork() { StepWorkId = 41, DurationDay = 13, RelatedProcessorStepWork = 48,Color = Color.Red}
        }
      };
      listOut.Add( task35 );
      var task36 = new Task()
      {
        TaskId = 33,
        TaskName = "地覆・壁高欄部鉄筋工",
        Description = null,
        NumberDay1 = 6,
        NumberTeam = 2,
        NumberDay2 = 5.1,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 42, DurationDay = 5.1, RelatedProcessorStepWork = 46, },
        }
      };
      listOut.Add( task36 );
      var task37 = new Task()
      {
        TaskId = 34,
        TaskName = "目地板設置工",
        Description = null,
        NumberDay1 = 0.2,
        NumberTeam = 1,
        NumberDay2 = 0.3,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 43, DurationDay = 0.3, RelatedProcessorStepWork = 23, },
        }
      };
      listOut.Add( task37 );
      var task38 = new Task()
      {
        TaskId = 35,
        TaskName = "床版ｺﾝｸﾘｰﾄ工",
        Description = "段取り+予備日",
        NumberDay1 = 2,
        NumberTeam = 0,
        NumberDay2 = 2,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "稼働率100%",
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 44, DurationDay = 2, RelatedProcessorStepWork = 58, },
        }
      };
      listOut.Add( task38 );
      var task39 = new Task()
      {
        TaskId = 36,
        TaskName = null,
        Description = "ｺﾝｸﾘｰﾄ工",
        NumberDay1 = 12.6,
        NumberTeam = 1,
        NumberDay2 = 21.4,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 45, DurationDay = 21.4, RelatedProcessorStepWork = 44, },
        }
      };
      listOut.Add( task39 );
      var task40 = new Task()
      {
        TaskId = 37,
        TaskName = "床版ｺﾝｸﾘｰﾄ養生工",
        Description = null,
        NumberDay1 = 5,
        NumberTeam = 0,
        NumberDay2 = 5,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 46, DurationDay = 5, RelatedProcessorStepWork = 45, },
        }
      };
      listOut.Add( task40 );
      var task41 = new Task()
      {
        TaskId = 38,
        TaskName = "地覆・壁高欄部ｺﾝｸﾘｰﾄ工",
        Description = null,
        NumberDay1 = 8.1,
        NumberTeam = 1,
        NumberDay2 = 13.8,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 47, DurationDay = 13.8, RelatedProcessorStepWork = 40, },
        }
      };
      listOut.Add( task41 );
      var task42 = new Task()
      {
        TaskId = 39,
        TaskName = "地覆・壁高欄部養生工",
        Description = null,
        NumberDay1 = 5,
        NumberTeam = 0,
        NumberDay2 = 5,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = "稼働率100%",
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 48, DurationDay = 5, RelatedProcessorStepWork = 47, },
        }
      };
      listOut.Add( task42 );
      var task43 = new Task()
      {
        TaskName = "橋面工",
      };
      listOut.Add( task43 );
      var task44 = new Task()
      {
        TaskId = 40,
        TaskName = "ｱｽﾌｧﾙﾄ舗装",
        Description = "表層t=40mm",
        NumberDay1 = 1.2,
        NumberTeam = 2,
        NumberDay2 = 1,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 49, DurationDay = 1, RelatedProcessorStepWork = 50, },
        }
      };
      listOut.Add( task44 );
      var task45 = new Task()
      {
        TaskId = 41,
        TaskName = null,
        Description = "表層t=40mm",
        NumberDay1 = 1.2,
        NumberTeam = 2,
        NumberDay2 = 1,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 50, DurationDay = 1, RelatedProcessorStepWork = 51, },
        }
      };
      listOut.Add( task45 );
      var task46 = new Task()
      {
        TaskId = 42,
        TaskName = "ｺﾝｸﾘｰﾄ舗装",
        Description = null,
        NumberDay1 = 3.3,
        NumberTeam = 2,
        NumberDay2 = 2.8,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 51, DurationDay = 2.8, RelatedProcessorStepWork = 52, },
        }
      };
      listOut.Add( task46 );
      var task47 = new Task()
      {
        TaskId = 43,
        TaskName = "防水層",
        Description = "ｼｰﾄ系",
        NumberDay1 = 14.3,
        NumberTeam = 2,
        NumberDay2 = 12.2,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 52, DurationDay = 12.2, RelatedProcessorStepWork = 57, },
        }
      };
      listOut.Add( task47 );
      var task48 = new Task()
      {
        TaskId = 44,
        TaskName = "排水装置",
        Description = "排水桝",
        NumberDay1 = 4.0,
        NumberTeam = 2,
        NumberDay2 = 3.4,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 53, DurationDay = 3.4, RelatedProcessorStepWork = 39, },
        }
      };
      listOut.Add( task48 );
      var task49 = new Task()
      {
        TaskId = 45,
        TaskName = null,
        Description = "排水桝",
        NumberDay1 = 30.1,
        NumberTeam = 2,
        NumberDay2 = 25.6,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 54, DurationDay = 25.6, RelatedProcessorStepWork = 55, },
        }
      };
      listOut.Add( task49 );
      var task50 = new Task()
      {
        TaskId = 46,
        TaskName = null,
        Description = "ｺﾝｸﾘｰﾄｱﾝｶｰﾎﾞﾙﾄ",
        NumberDay1 = 5.6,
        NumberTeam = 2,
        NumberDay2 = 4.8,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 55, DurationDay = 4.8, RelatedProcessorStepWork = 56, },
        }
      };
      listOut.Add( task50 );

      var task51 = new Task()
      {
        TaskId = 47,
        TaskName = "橋面排水",
        Description = "ﾌﾚｷｼﾌﾞﾙﾁｭｰﾌﾞ",
        NumberDay1 = 0.1,
        NumberTeam = 2,
        NumberDay2 = 0.2,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 56, DurationDay =0.2, RelatedProcessorStepWork = 41, },
        }
      };
      listOut.Add( task51 );
      var task52 = new Task()
      {
        TaskId = 48,
        TaskName = "落橋防止構造",
        Description = null,
        NumberDay1 = 2.5,
        NumberTeam = 1,
        NumberDay2 = 4.3,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 57, DurationDay =4.3, RelatedProcessorStepWork = 54, },
        }
      };
      listOut.Add( task52 );
      var task53 = new Task()
      {
        TaskId = 49,
        TaskName = "伸縮装置継手設置工",
        Description = "鋼ﾌｨﾝｶﾞｰｼﾞｮｲﾝﾄ",
        NumberDay1 = 1.5,
        NumberTeam = 1,
        NumberDay2 = 2.6,
        NumberDay3 = null,
        NumberDay4 = null,
        Remarks = null,
        StepWorks = new List<StepWork>()
        {
          new StepWork() { StepWorkId = 58, DurationDay =2.6, RelatedProcessorStepWork = 53, },
        }
      };
      listOut.Add( task53 );
      listOut.Add( new Task() { IsTask = false } );
      return listOut;
    }
  }
}