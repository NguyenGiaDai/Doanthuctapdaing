using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepotId = table.Column<int>(type: "int", nullable: false),
                    BlockCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BlockName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BlockType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaxBay = table.Column<int>(type: "int", nullable: true),
                    MaxRow = table.Column<int>(type: "int", nullable: true),
                    MaxTier = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blocks_Depots_DepotId",
                        column: x => x.DepotId,
                        principalTable: "Depots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "bd29d625-de10-4ebc-a2e6-6818cb38e71d", "AQAAAAIAAYagAAAAEKWnSa+GAsM2avNin7R5eGX+Gy9ONguiwl0uvHdOoGMjh2hmzYfIedbOyIkr7M4FQQ==" });

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_DepotId",
                table: "Blocks",
                column: "DepotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "3a774cc4-2e30-4815-b43d-e3c544b60c28", "AQAAAAIAAYagAAAAEFty6uJTJlXnaVMdQVckPwdknHIjD0/plyWsWg0uBkgWTwDBIZAnRC1CZnW0mTdN8A==" });
        }
    }
}
