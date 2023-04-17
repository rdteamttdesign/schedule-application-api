namespace SchedulingTool.Api.Domain.Models
{
  public partial class View
    {
        public View()
        {
            ViewTasks = new HashSet<ViewTask>();
        }

        public long ViewId { get; set; }
        public string ViewName { get; set; } = string.Empty;
        public long ProjectId { get; set; }

        public virtual Project Project { get; set; } = null!;
        public virtual ICollection<ViewTask> ViewTasks { get; set; }
    }
}
