using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContainerTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContainerTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContainerId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransactionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VehicleNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FromBlockId = table.Column<int>(type: "int", nullable: true),
                    FromBay = table.Column<int>(type: "int", nullable: true),
                    FromRow = table.Column<int>(type: "int", nullable: true),
                    FromTier = table.Column<int>(type: "int", nullable: true),
                    ToBlockId = table.Column<int>(type: "int", nullable: true),
                    ToBay = table.Column<int>(type: "int", nullable: true),
                    ToRow = table.Column<int>(type: "int", nullable: true),
                    ToTier = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContainerTransactions_Blocks_FromBlockId",
                        column: x => x.FromBlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainerTransactions_Blocks_ToBlockId",
                        column: x => x.ToBlockId,
                        principalTable: "Blocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContainerTransactions_Containers_ContainerId",
                        column: x => x.ContainerId,
                        principalTable: "Containers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "d271481b-7891-4367-bf0d-a9740c189624", "AQAAAAIAAYagAAAAEBv+7OPcpValWBHVfKMRy7bfWVB3hDdcDecsJx7wR2F7aBd83w+4Zubu3nOFs3Bc9A==" });

            migrationBuilder.CreateIndex(
                name: "IX_ContainerTransactions_ContainerId",
                table: "ContainerTransactions",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerTransactions_FromBlockId",
                table: "ContainerTransactions",
                column: "FromBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_ContainerTransactions_ToBlockId",
                table: "ContainerTransactions",
                column: "ToBlockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContainerTransactions");

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "7fbcfe4d-4993-4de0-9300-35aa2187a18f", "AQAAAAIAAYagAAAAEBjAT8Fr/po7UAbSlD+5K0xh/tn4C/7SM5VpToDsX57ckkVHjwF4/IrRJ3ecyja6kg==" });
        }
    }
}
