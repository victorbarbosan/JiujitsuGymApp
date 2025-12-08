using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JiujitsuGymApp.Migrations
{
	/// <inheritdoc />
	public partial class InitialCreate : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Products",
				columns: table => new
				{
					Id = table.Column<int>(type: "int", nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
					Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
					Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
					Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
					CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Products", x => x.Id);
				});

			migrationBuilder.InsertData(
				table: "Products",
				columns: new[] { "Id", "Category", "CreatedDate", "Description", "Name", "Price" },
				values: new object[,]
				{
					{ 1, "Gis", new DateTime(2025, 10, 22, 22, 16, 36, 721, DateTimeKind.Utc).AddTicks(898), "Premium Jiu-Jitsu Gi", "BJJ Gi White", 129.99m },
					{ 2, "Apparel", new DateTime(2025, 10, 22, 22, 16, 36, 721, DateTimeKind.Utc).AddTicks(2472), "Compression rash guard", "Rash Guard", 49.99m },
					{ 3, "Belts", new DateTime(2025, 10, 22, 22, 16, 36, 721, DateTimeKind.Utc).AddTicks(2476), "White belt for beginners", "BJJ Belt White", 24.99m },
					{ 4, "Accessories", new DateTime(2025, 10, 22, 22, 16, 36, 721, DateTimeKind.Utc).AddTicks(2478), "Protective tape for fingers", "Finger Tape", 9.99m },
					{ 5, "Safety", new DateTime(2025, 10, 22, 22, 16, 36, 721, DateTimeKind.Utc).AddTicks(2480), "Protective mouth guard", "Mouth Guard", 19.99m }
				});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Products");
		}
	}
}
