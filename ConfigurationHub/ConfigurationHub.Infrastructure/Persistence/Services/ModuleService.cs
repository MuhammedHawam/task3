using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;
using PartnersHub.ConfigurationHub.Domain.Enums;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Services;

public class ModuleService : IModuleService
{
    private readonly IModuleRepository _moduleRepository;

    public ModuleService(IModuleRepository moduleRepository)
    {
        _moduleRepository = moduleRepository;
    }

    public async Task<Module> CreateModuleAsync(string name, ModuleType moduleType, string description)
    {
        var module = new Module
        {
            Name = name,
            ModuleType = moduleType,
            Description = description,
            IsActive = true
        };
        return await _moduleRepository.AddAsync(module);
    }

    public async Task<IEnumerable<Module>> GetAllModulesAsync() => 
        await _moduleRepository.GetAllAsync();

    public async Task<IEnumerable<Module>> GetActiveModulesAsync() => 
        await _moduleRepository.GetActiveModulesAsync();

    public async Task<Module?> GetModuleByIdAsync(Guid moduleId) => 
        await _moduleRepository.GetByIdAsync(moduleId);

    public async Task<Module?> GetModuleByNameAsync(string moduleName) => 
        await _moduleRepository.GetByNameAsync(moduleName);

    public async Task<bool> UpdateModuleAsync(Guid moduleId, string name, string description)
    {
        var module = await _moduleRepository.GetByIdAsync(moduleId);
        if (module == null) return false;

        module.Name = name;
        module.Description = description;
        await _moduleRepository.UpdateAsync(module);
        return true;
    }

    public async Task<bool> DeleteModuleAsync(Guid moduleId) => 
        await _moduleRepository.DeleteAsync(moduleId);
}
