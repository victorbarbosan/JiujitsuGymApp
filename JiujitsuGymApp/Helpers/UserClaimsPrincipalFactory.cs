using System.Security.Claims;
using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace JiujitsuGymApp.Helpers
{
    public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User>
    {
        public UserClaimsPrincipalFactory(
            UserManager<User> userManager,
            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, optionsAccessor)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            // Add the FirstName claim so it's available in the Layout
            identity.AddClaim(new Claim("FirstName", user.FirstName ?? ""));
            identity.AddClaim(new Claim("LastName", user.LastName ?? ""));

            return identity;
        }
    }
}