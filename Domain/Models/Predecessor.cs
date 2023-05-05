using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class Predecessor
    {
        public long StepworkId { get; set; }
        public long RelatedStepworkId { get; set; }
        public long Type { get; set; }
        public float Lag { get; set; }
        public string RelatedStepworkLocalId { get; set; } = null!;

        public virtual PredecessorType TypeNavigation { get; set; } = null!;
    }
}
