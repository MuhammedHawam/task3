using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartnersHub.ConfigurationHub.Application.Common.DTOs
{
    public class RegisteredCompanyListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ModuleName { get; set; }
        public string SectorName { get; set; }
        public string? OnboardedBy { get; set; }
        public DateTime? OnboardingDate { get; set; }
    }

    public class RegisteredCompanyDto
    {
        public string CompanyId { get; set; }
        public string Name { get; set; }
    }

    public class CreateRegisteredCompanyDto
    {
        public string? sectorId {  get; set; }
        public string? sectorName { get; set; }
        public  Guid ModuleId { get; set; }
        public string description { get; set; }
        public List<RegisteredCompanyDto> compaines { get; set; }
    }
}
