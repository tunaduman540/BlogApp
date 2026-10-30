using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BlogApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "Password", "Username" },
                values: new object[] { 1, "tuna@example.com", "TempPassword123!", "tuna" });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Content", "IsActive", "PublishedOn", "Title", "UserId" },
                values: new object[,]
                {
                    { 1, "Bu benim ilk blog yazım.", true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "İlk Blog Yazım", 1 },
                    { 2, "ASP.NET Core ile web uygulamaları geliştirebiliriz.", true, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "ASP.NET Core Nedir?", 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
