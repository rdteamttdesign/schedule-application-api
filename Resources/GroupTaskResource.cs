namespace SchedulingTool.Api.Resources;

public class GroupTaskResource
{
  public decimal Start { get; set; }
  public decimal Duration { get; set; }
  public string Name { set; get; } = null!;
  public string Id { set; get; } = null!;
  public string Type { set; get; } = null!;
  public bool HideChildren { get; set; }
  public int DisplayOrder { get; set; }
  public long ColorId { set; get; }
  public int GroupsNumber { set; get; }
}
