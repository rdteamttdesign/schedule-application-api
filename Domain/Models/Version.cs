using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class Version
    {
        public Version()
        {
            ColorDefs = new HashSet<ColorDef>();
            GroupTasks = new HashSet<GroupTask>();
            ProjectBackgrounds = new HashSet<ProjectBackground>();
            Views = new HashSet<View>();
            Projects = new HashSet<Project>();
        }

        public long VersionId { get; set; }
        public string VersionName { get; set; } = null!;
        public long UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActivated { get; set; }
        public int NumberOfMonths { get; set; }

        public virtual ProjectSetting? ProjectSetting { get; set; }
        public virtual ICollection<ColorDef> ColorDefs { get; set; }
        public virtual ICollection<GroupTask> GroupTasks { get; set; }
        public virtual ICollection<ProjectBackground> ProjectBackgrounds { get; set; }
        public virtual ICollection<View> Views { get; set; }

        public virtual ICollection<Project> Projects { get; set; }
    }
}
