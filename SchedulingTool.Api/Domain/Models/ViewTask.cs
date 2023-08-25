namespace SchedulingTool.Api.Domain.Models
{
  public partial class ViewTask
    {
        public long ViewId { get; set; }
        public string LocalTaskId { get; set; } = null!;
        public int Group { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsHidden { get; set; }
        public string TaskName { get; set; } = null!;
        public string? TaskDescription { get; set; }
        public string? TaskNote { get; set; }
        public bool IsDayFormat { get; set; }

        public virtual View View { get; set; } = null!;
    }
}
