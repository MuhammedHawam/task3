using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using PartnersHub.ConfigurationHub.Domain.Enums;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;

public interface IModuleService
{
    Task<Module> CreateModuleAsync(string name, ModuleType moduleType, string description);
    Task<IEnumerable<Module>> GetAllModulesAsync();
    Task<IEnumerable<LookupDto>> GetLookupAsync();
    Task<IEnumerable<Module>> GetActiveModulesAsync();
    Task<Module?> GetModuleByIdAsync(Guid moduleId);
    Task<bool> UpdateModuleAsync(Guid moduleId, string name, string description);
    Task<bool> DeleteModuleAsync(Guid moduleId);
}
