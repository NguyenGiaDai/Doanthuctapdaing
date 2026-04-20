using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Book");

            migrationBuilder.DropTable(
                name: "ForgotPassword");

            migrationBuilder.CreateTable(
                name: "Depots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepotCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepotName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depots", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "3a774cc4-2e30-4815-b43d-e3c544b60c28", "AQAAAAIAAYagAAAAEFty6uJTJlXnaVMdQVckPwdknHIjD0/plyWsWg0uBkgWTwDBIZAnRC1CZnW0mTdN8A==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Depots");

            migrationBuilder.CreateTable(
                name: "Book",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Book", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForgotPassword",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OTP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForgotPassword", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "67c5d429-b48d-4b96-ad55-eb3591a52d59", "AQAAAAIAAYagAAAAEAmPSCf7BIMlxnhKUfUltg22NyiU31J9jKCofrhJA9oavON9Y0m1Ozz9k2H0OqpwCw==" });

            migrationBuilder.InsertData(
                table: "Book",
                columns: new[] { "Id", "Description", "Price", "Title" },
                values: new object[,]
                {
                    { 1, "A comprehensive guide to C# programming.", 29.989999999999998, "C# Programming" },
                    { 2, "Learn how to build web applications using ASP.NET Core.", 35.5, "ASP.NET Core Development" },
                    { 3, "Master the Entity Framework Core ORM for .NET development.", 40.0, "Entity Framework Core In Action" },
                    { 4, "Everything you need to know about building Blazor WebAssembly applications.", 45.990000000000002, "Blazor WebAssembly: The Complete Guide" },
                    { 5, "Implement common design patterns in C# to improve code structure.", 50.0, "Design Patterns in C#" }
                });
        }
    }
}
