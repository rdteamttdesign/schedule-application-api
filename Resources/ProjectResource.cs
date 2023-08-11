using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.ExportExcel;

namespace SchedulingTool.Api.Resources;

public class ProjectResource
{
  public string ProjectName { get; set; } = null!;
  public ProjectSetting Setting { get; set; } = null!;
  public IList<ColorDef> UsedColors { get; set; } = null!;
  public Dictionary<View, List<ViewTaskDetail>> ViewTasks { get; set; } = null!;
  public IList<ChartStepwork> ChartStepworks { get; set; } = null!;
  public IList<GroupTaskDetailResource> Grouptasks { get; set; } = null!;
  public IList<ProjectBackgroundResource> Backgrounds { get; set; } = null!;
}
