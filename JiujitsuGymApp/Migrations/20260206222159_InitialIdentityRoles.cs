using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JiujitsuGymApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentityRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Category", "CreatedDate", "Description", "Name", "Price" },
                values: new object[,]
                {
                    { 1, "Gis", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Premium Jiu-Jitsu Gi", "BJJ Gi White", 129.99m },
                    { 2, "Apparel", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Compression rash guard", "Rash Guard", 49.99m },
                    { 3, "Belts", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "White belt for beginners", "BJJ Belt White", 24.99m },
                    { 4, "Accessories", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Protective tape for fingers", "Finger Tape", 9.99m },
                    { 5, "Safety", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Protective mouth guard", "Mouth Guard", 19.99m }
                });
        }
    }
}
