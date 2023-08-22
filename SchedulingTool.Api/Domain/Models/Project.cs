using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class Project
    {
        public Project()
        {
            ProjectVersions = new HashSet<ProjectVersion>();
        }

        public long ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;
        public long UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsShared { get; set; }
        public string ModifiedBy { get; set; } = null!;

        public virtual ICollection<ProjectVersion> ProjectVersions { get; set; }
    }
}
