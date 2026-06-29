using JiujitsuGymApp.Data;
using Microsoft.EntityFrameworkCore;

namespace JiujitsuGymApp.Services
{
    public class CurrentUserService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext db)
        : ICurrentUserService
    {
        public async Task<string?> GetUserIdAsync()
        {
            var userName = httpContextAccessor.HttpContext?.User.Identity?.Name;
            if (userName is null) return null;

            return await db.Users
                .Where(u => u.UserName == userName)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
        }
    }
}