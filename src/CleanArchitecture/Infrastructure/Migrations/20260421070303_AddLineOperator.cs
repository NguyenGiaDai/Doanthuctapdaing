using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLineOperator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LineOperators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LineOperatorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LineOperatorName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineOperators", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "3ceb2e15-1cf9-45aa-94bc-4f86497d7cbc", "AQAAAAIAAYagAAAAEPu3PoZ6JR0Q9Sh2B0kTxcKqy2TOA9SrzmCuGTxw4MaTRDZrCFyOvxKKWjKhrhnisg==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LineOperators");

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "84553559-01ae-425e-8a24-365e2337a92a", "AQAAAAIAAYagAAAAEMbu3zKS71iDOiQy+DQmbCl1ZxMK5m8VUdHj59SQ8xaknndWUWE5YF/Dhcc/PmPA8A==" });
        }
    }
}
