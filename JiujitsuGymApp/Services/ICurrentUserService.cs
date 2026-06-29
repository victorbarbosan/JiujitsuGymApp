namespace JiujitsuGymApp.Services
{
    public interface ICurrentUserService
    {
        Task<string?> GetUserIdAsync();
    }
}