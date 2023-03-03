using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class ViewTask
    {
        public long ViewId { get; set; }
        public long TaskId { get; set; }
        public int Group { get; set; }

        public virtual Task Task { get; set; } = null!;
        public virtual View View { get; set; } = null!;
    }
}
