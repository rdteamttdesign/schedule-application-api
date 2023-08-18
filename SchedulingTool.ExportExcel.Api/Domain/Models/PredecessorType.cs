using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class PredecessorType
    {
        public PredecessorType()
        {
            Predecessors = new HashSet<Predecessor>();
        }

        public long PredecessorTypeId { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;

        public virtual ICollection<Predecessor> Predecessors { get; set; }
    }
}
