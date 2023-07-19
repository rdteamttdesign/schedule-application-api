using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class Project
    {
        public Project()
        {
            Versions = new HashSet<Version>();
        }

        public long ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;

        public virtual ICollection<Version> Versions { get; set; }
    }
}
