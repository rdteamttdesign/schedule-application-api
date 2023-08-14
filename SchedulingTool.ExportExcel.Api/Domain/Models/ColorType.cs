using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class ColorType
    {
        public ColorType()
        {
            ColorDefs = new HashSet<ColorDef>();
        }

        public long ColorTypeId { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<ColorDef> ColorDefs { get; set; }
    }
}
