using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;

public interface IModuleRepository
{
    Task<Module> AddAsync(Module module);
    Task<IEnumerable<Module>> GetAllAsync();
    Task<IEnumerable<LookupDto>> GetLookupAsync();
    Task<IEnumerable<Module>> GetActiveModulesAsync();
    Task<Module?> GetByIdAsync(Guid id);
    Task<Module?> GetByNameAsync(string name);
    Task UpdateAsync(Module module);
    Task<bool> DeleteAsync(Guid id);
}
