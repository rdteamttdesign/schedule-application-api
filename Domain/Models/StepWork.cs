﻿using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class Stepwork
    {
        public long StepworkId { get; set; }
        public int Index { get; set; }
        public decimal Portion { get; set; }
        public long TaskId { get; set; }
        public long ColorId { get; set; }
        public string LocalId { get; set; } = null!;
        public string TaskLocalId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Start { get; set; }
        public decimal End { get; set; }
        public decimal Duration { get; set; }
        public ulong Type { get; set; }

        public virtual ColorDef Color { get; set; } = null!;
        public virtual Task Task { get; set; } = null!;
    }
}
