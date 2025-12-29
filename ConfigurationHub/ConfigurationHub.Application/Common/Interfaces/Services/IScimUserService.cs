using PartnersHub.ConfigurationHub.Application.Common.Models;

namespace PartnersHub.ConfigurationHub.Application.Common.Interfaces.Services;

/// <summary>
/// Service for managing external users via CIAM SCIM API
/// Used for external partner companies and their users
/// </summary>
public interface IScimUserService
{
    Task<List<SimpleUser>> GetUsersAsync();
}
