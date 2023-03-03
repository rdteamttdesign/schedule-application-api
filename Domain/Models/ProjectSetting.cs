using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class ProjectSetting
    {
        public long ProjectSettingId { get; set; }
        public long ProjectId { get; set; }
        public bool SeparateGroupTask { get; set; }
        public float AssemblyDurationRatio { get; set; }
        public float RemovalDurationRatio { get; set; }

        public virtual Project Project { get; set; } = null!;
    }
}
