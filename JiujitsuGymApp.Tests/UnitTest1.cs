namespace JiujitsuGymApp.Tests
{
	public class UnitTest1
	{

		[Theory]
		[InlineData(2, 3, 5)]
		[InlineData(10, 5, 15)]
		[InlineData(-1, 1, 0)]
		public void Add_ShouldReturnCorrectValue(int a, int b, int expected)
		{
			// Act
			int result = a + b;

			// Assert
			Assert.Equal(expected, result);
		}

		[Theory]
		[InlineData("Victor", true)]
		[InlineData("", false)]
		public void IsValidName(string name, bool expected)
		{
			bool result = !string.IsNullOrWhiteSpace(name);
			Assert.Equal(expected, result);
		}

		[InlineData(5, true)]
		[InlineData(17, true)]
		[InlineData(4, false)]
		[InlineData(28, false)]
		public void IsNumberOdd(int number, bool expected)
		{
			bool isOdd = number % 2 != 0;
			Assert.Equal(expected, isOdd);
		}
	}
}