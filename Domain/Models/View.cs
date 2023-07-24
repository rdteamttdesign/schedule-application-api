using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class View
    {
        public View()
        {
            ViewTasks = new HashSet<ViewTask>();
        }

        public long ViewId { get; set; }
        public string ViewName { get; set; } = null!;
        public long VersionId { get; set; }

        public virtual Version Version { get; set; } = null!;
        public virtual ICollection<ViewTask> ViewTasks { get; set; }
    }
}
