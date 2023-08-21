namespace SchedulingTool.Api.Resources.FormBody;

public class SendVersionsToAnotherProjectFormData
{
  public ICollection<long> VersionIds { get; set; } = null!;
  public long? ToProjectId { get; set; }
  public bool SendToSharedProjectList { get; set; }
  public bool CreateCopy { get; set; }
}
