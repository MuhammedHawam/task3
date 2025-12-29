public interface IUserProfileDataIntegrationService
{
    Task<UserProfileDataDto?> GetUserProfileData();

}