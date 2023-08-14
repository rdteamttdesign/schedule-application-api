namespace SchedulingTool.Api.Resources.Extended;

public class StepworkDetailResource : IEquatable<StepworkDetailResource>
{
  public string LocalId { get; set; } = null!;
  public double Portion { get; set; }
  public long ColorId { get; set; }

  public bool Equals( StepworkDetailResource? other )
  {
    if ( other == null )
      return false;

    return Math.Round( Portion, 6 ) == Math.Round( other.Portion, 6 )
      && ColorId.Equals( other.ColorId );
  }
}
