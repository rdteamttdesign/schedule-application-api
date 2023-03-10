using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class Stepwork
    {
        public Stepwork()
        {
            PredecessorRelatedStepworks = new HashSet<Predecessor>();
            PredecessorStepworks = new HashSet<Predecessor>();
        }

        public long StepworkId { get; set; }
        public int Index { get; set; }
        public float Duration { get; set; }
        public long TaskId { get; set; }
        public long ColorId { get; set; }

        public virtual ColorDef Color { get; set; } = null!;
        public virtual Task Task { get; set; } = null!;
        public virtual ICollection<Predecessor> PredecessorRelatedStepworks { get; set; }
        public virtual ICollection<Predecessor> PredecessorStepworks { get; set; }
    }
}
