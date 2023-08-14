using System;
using System.Collections.Generic;

namespace SchedulingTool.Api.Domain.Models
{
    public partial class User
    {
        public long UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Avatar { get; set; }
        public ulong? IsOnline { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
