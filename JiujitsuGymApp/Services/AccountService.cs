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

        private async Task RecordLoginAsync(User user)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
        }
    }
}
