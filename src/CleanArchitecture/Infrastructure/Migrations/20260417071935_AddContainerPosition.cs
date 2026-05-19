using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContainerPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContainerPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContainerId = table.Column<int>(type: "int", nullable: false),
                    BlockId = table.Column<int>(type: "int", nullable: false),
                    Bay = table.Column<int>(type: "int", nullable: false),
                    Row = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    PositionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerPositions_Blocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContainerPositions_Containers_ContainerId",
                        column: x => x.ContainerId,
                        principalTable: "Containers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "7fbcfe4d-4993-4de0-9300-35aa2187a18f", "AQAAAAIAAYagAAAAEBjAT8Fr/po7UAbSlD+5K0xh/tn4C/7SM5VpToDsX57ckkVHjwF4/IrRJ3ecyja6kg==" });

            migrationBuilder.CreateIndex(
                name: "IX_ContainerPositions_BlockId",
                table: "ContainerPositions",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerPositions_ContainerId",
                table: "ContainerPositions",
                column: "ContainerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContainerPositions");

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "bd29d625-de10-4ebc-a2e6-6818cb38e71d", "AQAAAAIAAYagAAAAEKWnSa+GAsM2avNin7R5eGX+Gy9ONguiwl0uvHdOoGMjh2hmzYfIedbOyIkr7M4FQQ==" });
        }
    }
}
