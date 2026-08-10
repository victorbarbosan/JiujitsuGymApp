using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Identity;

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
