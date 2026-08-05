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

            // Belt drives the avatar colour in the navbar, which renders on
            // every page. A claim keeps that from costing a user lookup per
            // request. It is baked into the auth cookie, so a promotion does
            // not show here until the principal is regenerated - on the next
            // sign-in, or when the security stamp is next validated.
            identity.AddClaim(new Claim("Belt", (user.Belt ?? BeltColor.White).ToString()));

            return identity;
        }
    }
}