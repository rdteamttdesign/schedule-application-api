using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class Project
    {
        public long ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;

        public virtual ProjectVersion? ProjectVersion { get; set; }
    }
}
