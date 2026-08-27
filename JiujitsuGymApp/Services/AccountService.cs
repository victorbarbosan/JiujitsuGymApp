using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace JiujitsuGymApp.Services
{
    public class AccountService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        UserService userService)
    {
        public async Task<bool> LoginAsync(string email, string password, bool rememberMe)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null) return false;

            var result = await signInManager.PasswordSignInAsync(
                user.UserName!, password, rememberMe, lockoutOnFailure: false);
            if (!result.Succeeded) return false;

            await RecordLoginAsync(user);
            return true;
        }

        public async Task<IEnumerable<string>> RegisterAsync(CreateUserDto dto)
        {
            var (_, errors) = await userService.CreateUserAsync(dto);
            if (errors.Any()) return errors;

            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user is null) return ["Unable to sign in the new account."];

            await signInManager.SignInAsync(user, isPersistent: false);
            await RecordLoginAsync(user);
            return [];
        }

        public Task LogoutAsync() => signInManager.SignOutAsync();

        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string? redirectUrl) =>
            signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        public Task<ExternalLoginInfo?> GetExternalLoginInfoAsync() => signInManager.GetExternalLoginInfoAsync();

        /// <summary>
        /// Attempts sign-in with an existing external login link. Returns the
        /// sign-in result so the caller can distinguish success, lockout, and
        /// "not linked yet" (which falls back to provisioning below).
        /// </summary>
        public async Task<SignInResult> ExternalLoginSignInAsync(ExternalLoginInfo info)
        {
            var result = await signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user is not null) await RecordLoginAsync(user);
            }

            return result;
        }

        /// <summary>
        /// Creates (or links to an existing email match) a local account from
        /// external login claims, then signs the user in. Used the first time
        /// someone authenticates via Google without a prior local account.
        /// </summary>
        public async Task<IEnumerable<string>> ProvisionExternalUserAsync(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return ["The external provider did not supply an email address."];

            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                var dto = new CreateUserDto
                {
                    FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? email,
                    LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
                    Email = email,
                    PhoneNumber = string.Empty,
                    Belt = "White",
                    Password = Guid.NewGuid().ToString("N") + "aA1!",
                    Role = "Member"
                };

                var (_, errors) = await userService.CreateUserAsync(dto);
                if (errors.Any()) return errors;

                user = await userManager.FindByEmailAsync(email);
                if (user is null) return ["Unable to create an account for this Google sign-in."];
            }

            var addLoginResult = await userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
                return addLoginResult.Errors.Select(e => e.Description);

            await signInManager.SignInAsync(user, isPersistent: false);
            await RecordLoginAsync(user);
            return [];
        }

        /// <summary>
        /// Returns a password reset token for the account, or null when no
        /// account holds this address. Callers must not surface which of the
        /// two happened — that would let anyone probe for registered emails.
        /// </summary>
        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null) return null;

            return await userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IEnumerable<string>> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
                return ["This password reset link is invalid or has expired. Please request a new one."];

            var result = await userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded) return [];

            // A bad or expired token needs friendlier wording than Identity's
            // "Invalid token."; password-rule failures read fine as-is.
            return result.Errors.Select(e => e.Code == "InvalidToken"
                ? "This password reset link is invalid or has expired. Please request a new one."
                : e.Description);
        }

        private async Task RecordLoginAsync(User user)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
        }
    }
}
