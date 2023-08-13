﻿using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class Task
    {
        public Task()
        {
            Stepworks = new HashSet<Stepwork>();
        }

        public long TaskId { get; set; }
        public string TaskName { get; set; } = null!;
        public int Index { get; set; }
        public int NumberOfTeam { get; set; }
        public double Duration { get; set; }
        public double AmplifiedDuration { get; set; }
        public long GroupTaskId { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
        public string LocalId { get; set; } = null!;
        public string GroupTaskLocalId { get; set; } = null!;

        public virtual GroupTask GroupTask { get; set; } = null!;
        public virtual ICollection<Stepwork> Stepworks { get; set; }
    }
}
