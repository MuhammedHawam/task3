using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.ConfigurationHub.Application.Common.DTOs
{
    public class AdminUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string AssignedBy { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }

        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string ProductName { get; set; }
    }
}
