namespace PartnersHub.InfraBase.Application.Common.Interfaces;

public interface ITokenService
{
    string GetUserEmail();
    string GetUserName(); // Extract username from email (part before @)
    Guid? GetCompanyId();  // Changed to nullable
    string? GetCompanyName();
    List<Guid> GetUserRoleIds();
    bool IsPcAdmin();
    //bool IsInfrabaseAdmin();
}
