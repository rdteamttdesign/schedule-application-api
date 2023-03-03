using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class ColorDef
    {
        public ColorDef()
        {
            ProjectBackgrounds = new HashSet<ProjectBackground>();
            Stepworks = new HashSet<Stepwork>();
        }

        public long ColorId { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public long Type { get; set; }
        public long ProjectId { get; set; }
        public bool IsDefault { get; set; }

        public virtual Project Project { get; set; } = null!;
        public virtual ColorType TypeNavigation { get; set; } = null!;
        public virtual ICollection<ProjectBackground> ProjectBackgrounds { get; set; }
        public virtual ICollection<Stepwork> Stepworks { get; set; }
    }
}
