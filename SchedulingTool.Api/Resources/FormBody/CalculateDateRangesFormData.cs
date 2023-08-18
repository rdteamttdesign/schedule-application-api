namespace SchedulingTool.Api.Resources.FormBody;

public class CalculateDateRangesFormData
{
  public bool IncludeYear { get; set; }
  public int StartYear { get; set; }
  public int StartMonth { get; set; }
  public int NumberOfMonths { get; set; }
  public ICollection<BackgroundColorResource> BackgroundColors { get; set; } = null!;
}
