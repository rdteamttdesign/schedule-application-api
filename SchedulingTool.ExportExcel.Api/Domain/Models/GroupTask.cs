using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class GroupTask
    {
        public GroupTask()
        {
            Tasks = new HashSet<Task>();
        }

        public long GroupTaskId { get; set; }
        public string GroupTaskName { get; set; } = null!;
        public long VersionId { get; set; }
        public int Index { get; set; }
        public bool HideChidren { get; set; }
        public string LocalId { get; set; } = null!;

        public virtual Version Version { get; set; } = null!;
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
