using Microsoft.EntityFrameworkCore;
using PartnersHub.ConfigurationHub.Application.Common.DTOs;
using PartnersHub.ConfigurationHub.Application.Common.Interfaces.Persistence;
using PartnersHub.ConfigurationHub.Domain.Aggregates.RolesAndPermission;

namespace PartnersHub.ConfigurationHub.Infrastructure.Persistence.Repositories;

public class ModuleRepository : IModuleRepository
{
    private readonly ConfigurationHubDbContext _context;

    public ModuleRepository(ConfigurationHubDbContext context)
    {
        _context = context;
    }

    public async Task<Module> AddAsync(Module module)
    {
        await _context.Modules.AddAsync(module);
        await _context.SaveChangesAsync();
        return module;
    }

    public async Task<IEnumerable<Module>> GetAllAsync() =>
        await _context.Modules.ToListAsync();

    public async Task<IEnumerable<LookupDto>> GetLookupAsync() =>
    await _context.Modules.Select(a=> new LookupDto
    {
        Id = a.Id,
        Value = a.Name
    }).ToListAsync();

    public async Task<IEnumerable<Module>> GetActiveModulesAsync() =>
        await _context.Modules.Where(m => m.IsActive).ToListAsync();

    public async Task<Module?> GetByIdAsync(Guid id) =>
        await _context.Modules.FindAsync(id);

    public async Task<Module?> GetByNameAsync(string name) =>
        await _context.Modules.FirstOrDefaultAsync(m => m.Name == name);

    public async Task UpdateAsync(Module module)
    {
        _context.Modules.Update(module);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var module = await _context.Modules.FindAsync(id);
        if (module == null) return false;

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync();
        return true;
    }
}
