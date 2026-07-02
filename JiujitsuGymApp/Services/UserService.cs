using JiujitsuGymApp.Dtos;
using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Identity;

namespace JiujitsuGymApp.Services
{
    public class UserService(UserManager<User> userManager)
    {
        private static readonly HashSet<string> AllowedRoles = ["Admin", "Member", "Teacher"];

        public async Task<(UserDto? user, IEnumerable<string> errors)> CreateUserAsync(CreateUserDto dto)
        {
            if (!AllowedRoles.Contains(dto.Role))
                return (null, [$"Invalid role '{dto.Role}'."]);

            if (await userManager.FindByEmailAsync(dto.Email) is not null)
                return (null, ["A user with this email already exists."]);

            var beltColor = Enum.TryParse<BeltColor>(dto.Belt, out var parsed) ? parsed : BeltColor.White;

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Belt = beltColor,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return (null, result.Errors.Select(e => e.Description));

            await userManager.AddToRoleAsync(user, dto.Role);
            return (ToDto(user, dto.Role), []);
        }

        public async Task<(UserDto? user, IEnumerable<string> errors)> UpdateUserAsync(string id, UpdateUserDto dto)
        {
            if (!AllowedRoles.Contains(dto.Role))
                return (null, [$"Invalid role '{dto.Role}'."]);

            var user = await userManager.FindByIdAsync(id);
            if (user is null) return (null, ["User not found."]);

            if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await userManager.SetEmailAsync(user, dto.Email);
                if (!emailResult.Succeeded)
                    return (null, emailResult.Errors.Select(e => e.Description));

                await userManager.SetUserNameAsync(user, dto.Email);
            }

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;

            if (Enum.TryParse<BeltColor>(dto.Belt, out var belt))
                user.Belt = belt;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return (null, updateResult.Errors.Select(e => e.Description));

            var currentRoles = await userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(dto.Role))
            {
                await userManager.RemoveFromRolesAsync(user, currentRoles);
                await userManager.AddToRoleAsync(user, dto.Role);
            }

            return (ToDto(user, dto.Role), []);
        }

        public async Task<UserDto?> GetUserByIdAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null) return null;

            var roles = await userManager.GetRolesAsync(user);
            return ToDto(user, roles.FirstOrDefault() ?? "Member");
        }

        public static UserDto ToDto(User u, string role = "Member") => new()
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email!,
            PhoneNumber = u.PhoneNumber,
            Belt = u.Belt.HasValue ? u.Belt.Value.ToString() : "Not Set",
            Role = role
        };
    }
}