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
        public long VersionId { get; set; }
        public bool IsDefault { get; set; }
        public int IsInstall { get; set; }

        public virtual ColorType TypeNavigation { get; set; } = null!;
        public virtual Version Version { get; set; } = null!;
        public virtual ICollection<ProjectBackground> ProjectBackgrounds { get; set; }
        public virtual ICollection<Stepwork> Stepworks { get; set; }
    }
}
