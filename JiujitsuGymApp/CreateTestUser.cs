using JiujitsuGymApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace JiujitsuGymApp
{
	public class CreateTestUser
	{
		public static async Task CreateTestUserAsync(IServiceProvider serviceProvider)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

				// check if test user already exists
				var testUser = await userManager.FindByEmailAsync("test@jiujitsu.com");
				if (testUser == null)
				{
					testUser = new User
					{
						FirstName = "Test",
						LastName = "User",
						UserName = "test@jiujitsu.com",
						Email = "test@jiujitsu.com",
						Belt = BeltColor.Blue,
						PhoneNumber = "123-456-7890",
						CreatedAt = DateTime.Now
					};

					var result = await userManager.CreateAsync(testUser, "Abc123456!");

					if (result.Succeeded)
					{
						Console.WriteLine("✅ Test user created successfully!");
						Console.WriteLine($"   Email: test@jiujitsu.com");
						Console.WriteLine($"   Password: Test123!");
					}
					else
					{
						Console.WriteLine("❌ Failed to create test user:");
						foreach (var error in result.Errors)
						{
							Console.WriteLine($"   - {error.Description}");
						}
					}
				}
				else
				{
					Console.WriteLine("ℹ️ Test user already exists.");
				}

			}
		}
	}
}
