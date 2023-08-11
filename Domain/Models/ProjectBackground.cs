namespace SchedulingTool.Api.Domain.Models
{
  public partial class ProjectBackground
    {
        public long VersionId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int Date { get; set; }
        public long? ColorId { get; set; }

        public virtual ColorDef? Color { get; set; }
        public virtual Version Version { get; set; } = null!;
    }
}
