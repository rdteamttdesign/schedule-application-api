using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class ViewTask
    {
        public long ViewId { get; set; }
        public string LocalTaskId { get; set; } = null!;
        public int Group { get; set; }

        public virtual View View { get; set; } = null!;
    }
}
