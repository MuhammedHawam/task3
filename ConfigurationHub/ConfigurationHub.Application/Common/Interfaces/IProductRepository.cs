using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces;

public interface IModuleRepository
{
    Task<Module> AddAsync(Module module);
    Task<IEnumerable<Module>> GetAllAsync();
    Task<bool> CheckByNameAsync(string name);
}
