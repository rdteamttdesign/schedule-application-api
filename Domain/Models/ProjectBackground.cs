using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class ProjectBackground
    {
        public long ProjectId { get; set; }
        public int Month { get; set; }
        public long? ColorId { get; set; }

        public virtual ColorDef? Color { get; set; }
        public virtual Project Project { get; set; } = null!;
    }
}
