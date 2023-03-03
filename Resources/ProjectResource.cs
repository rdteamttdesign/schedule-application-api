namespace SchedulingTool.Api.Resources;

public class ProjectResource
{
  public long ProjectId { get; set; }
  public string ProjectName { get; set; } = null!;
  public long UserId { get; set; }
  public DateTime CreatedDate { get; set; }
  public DateTime? ModifiedDate { get; set; }
  public bool IsActivated { get; set; }
  public int NumberOfMonths { get; set; }
}
