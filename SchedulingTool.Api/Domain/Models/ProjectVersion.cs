using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class ProjectVersion
    {
        public long ProjectId { get; set; }
        public long VersionId { get; set; }

        public virtual Project Project { get; set; } = null!;
        public virtual Version Version { get; set; } = null!;
    }
}
