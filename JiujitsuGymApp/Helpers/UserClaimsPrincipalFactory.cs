using System.Security.Claims;
using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace JiujitsuGymApp.Helpers
{
    public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, IdentityRole>
    {
        public UserClaimsPrincipalFactory(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Add FirstName/LastName so they are available in views/layout
            identity.AddClaim(new Claim("FirstName", user.FirstName ?? ""));
            identity.AddClaim(new Claim("LastName", user.LastName ?? ""));

            return identity;
        }
    }
}