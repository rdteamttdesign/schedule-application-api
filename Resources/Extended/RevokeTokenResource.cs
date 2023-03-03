using System.ComponentModel.DataAnnotations ;

namespace SchedulingTool.Api.Resources.Extended ;

public class RevokeTokenResource
{
  [Required]
  public string Token { get; set; }

  public bool EventLogOut { get ; set ; } = false ;
}