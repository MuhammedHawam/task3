using PartnersHub.ConfigurationHub.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.ConfigurationHub.Application.Common.DTOs
{
    public class ProductPermissionDto
    {
        public string product {  get; set; }
        public IEnumerable<string> Permissions { get; set; }
    }
}
