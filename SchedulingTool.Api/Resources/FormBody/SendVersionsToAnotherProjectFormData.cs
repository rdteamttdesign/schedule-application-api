namespace SchedulingTool.Api.Resources.FormBody;

public class SendVersionsToAnotherProjectFormData
{
  public ICollection<long> VersionIds { get; set; } = null!;
  public long ToProject { get; set; }
}
