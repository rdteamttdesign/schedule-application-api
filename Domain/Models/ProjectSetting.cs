using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class ProjectSetting
    {
        public long VersionId { get; set; }
        public bool SeparateGroupTask { get; set; }
        public double AssemblyDurationRatio { get; set; }
        public double RemovalDurationRatio { get; set; }
        public int ColumnWidth { get; set; }
        public double AmplifiedFactor { get; set; }
        public bool IncludeYear { get; set; }
        public int StartYear { get; set; }
        public int StartMonth { get; set; }

        public virtual Version Version { get; set; } = null!;
    }
}
