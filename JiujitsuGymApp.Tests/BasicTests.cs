using JiujitsuGymApp.Models;

namespace JiujitsuGymApp.Tests;

public class BasicTests
{
	[Fact]
	public void Test_ClassModel_Creation()
	{
		// Arrange
		var teacher = new JiujitsuGymApp.Models.User { Id = "1", FirstName = "Victor" };
		var classTime = new DateTime(2025, 1, 1, 10, 0, 0);

		var model = new JiujitsuGymApp.Models.Class
		{
			TeacherId = teacher.Id,
			Teacher = teacher,
			Location = "Kitchener",
			DateTime = classTime
		};

		// Act + Assert
		Assert.Equal("1", model.TeacherId);
		Assert.Equal("Kitchener", model.Location);
		Assert.Equal(classTime, model.DateTime);
		Assert.True(model.CreatedAt <= DateTime.UtcNow);
		Assert.Null(model.DeletedAt);
		Assert.Same(teacher, model.Teacher);
	}

	[Fact]
	public void Test_UserModel_Creation()
	{
		// Arrange
		var user = new JiujitsuGymApp.Models.User
		{
			Id = "2",
			FirstName = "Victor",
			Email = "Victor@example.com",
			PhoneNumber = "123-456-7890",
			Belt = BeltColor.Blue,
			CreatedAt = DateTime.UtcNow
		};

		// Act + Assert
		Assert.Equal("2", user.Id);
		Assert.Equal("Victor", user.FirstName);
		Assert.Equal("Victor@example.com", user.Email);
		Assert.Equal("123-456-7890", user.PhoneNumber);
		Assert.Equal( "Blue", BeltColor.Blue.ToString());
		Assert.NotEqual(default, user.CreatedAt);
	}

}
