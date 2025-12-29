using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.InfraBase.Application.Common.Models
{
    public class PermssionDto
    {
        public string product { get; set; }
        public IEnumerable<string> Permissions { get; set; }
    }
}
