namespace PartnersHub.InfraBase.Application.Common.Interfaces;

public interface IUserDisplayNameService
{
    Task<string> ResolveDisplayNameAsync(
        Guid? contactId = null,
        CancellationToken cancellationToken = default);
}
