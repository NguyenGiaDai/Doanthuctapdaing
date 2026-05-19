using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContainerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Containers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContainerNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContainerType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsoCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContainerSize = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaximumWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TareWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DateOfManufacture = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContainerOwner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContainerCondition = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Containers", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "67c5d429-b48d-4b96-ad55-eb3591a52d59", "AQAAAAIAAYagAAAAEAmPSCf7BIMlxnhKUfUltg22NyiU31J9jKCofrhJA9oavON9Y0m1Ozz9k2H0OqpwCw==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Containers");

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "beb69596-7f7a-4150-b83f-c26fe411db7a", "AQAAAAIAAYagAAAAEFRvVcaq1XPSnKP9ULtz1SydbglqHOayROrNqAq7Nb4CrKahYze9DKJsmdXXUsJ6Uw==" });
        }
    }
}
