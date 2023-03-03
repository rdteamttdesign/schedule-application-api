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
        public long ProjectId { get; set; }

        public virtual Project Project { get; set; } = null!;
        public virtual ICollection<Task> Tasks { get; set; }
    }
}
