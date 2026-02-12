using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces;
using PartnersHub.ConfigurationHub.Application.Common.Models;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Apis.Controllers.Admin
{

    [ApiController]
    [Route("api/admin/registeredcompany")]
    [Authorize]

    public class RegisteredCompanyController : ControllerBase
    {
        private readonly IRegisteredCompanyRepository _registeredCompanyRepository ;

        public RegisteredCompanyController(IRegisteredCompanyRepository registeredCompanyRepository)
        {
            _registeredCompanyRepository = registeredCompanyRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<RegisteredCompanyListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedList<RegisteredCompanyListDto>>> GetAllCompanies(int pageNumber = 1, int pageSize = 10,string? searchTerm = null, string? sortBy = null)
        {
            var companies = await _registeredCompanyRepository.GetAllAsync(pageSize,pageNumber, searchTerm, sortBy);
            return Ok(companies);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] CreateRegisteredCompanyDto compainesdto)
        {
            var role = await _registeredCompanyRepository.AddAsync(compainesdto.ModuleId, compainesdto.sectorId, compainesdto.sectorName, compainesdto.description, compainesdto.compaines);
            return CreatedAtAction(nameof(CreateCompany), role);
        }

        [HttpDelete("{companyId}")]
        public async Task<IActionResult> DeleteRole(Guid companyId)
        {
            var success = await _registeredCompanyRepository.DeleteAsync(companyId);
            return success ? Ok(new { message = "Company deleted successfully" }) : NotFound(new { message = "Company not found" });
        }
    }
}
