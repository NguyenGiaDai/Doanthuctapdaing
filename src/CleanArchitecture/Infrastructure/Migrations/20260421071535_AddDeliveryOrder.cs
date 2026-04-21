using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DONumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContainerTypeId = table.Column<int>(type: "int", nullable: false),
                    LineOperatorId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VesselVoyage = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OrderStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryOrders_ContainerTypes_ContainerTypeId",
                        column: x => x.ContainerTypeId,
                        principalTable: "ContainerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryOrders_LineOperators_LineOperatorId",
                        column: x => x.LineOperatorId,
                        principalTable: "LineOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "34cf3477-bd6c-45e2-8ebb-d3995deca2fc", "AQAAAAIAAYagAAAAEC+sIUEGvaU6A8cLjSEos9vxJmLCgnAQLFCLCWAwbWeFAeM4gZndgXrSDGE5c5bdkw==" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrders_ContainerTypeId",
                table: "DeliveryOrders",
                column: "ContainerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrders_CustomerId",
                table: "DeliveryOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrders_LineOperatorId",
                table: "DeliveryOrders",
                column: "LineOperatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryOrders");

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "82607d96-7d29-469b-b3d6-f1a765accdcd", "AQAAAAIAAYagAAAAEHQ22ftG2HXIu1AYOzqt5RORVnETSuUif6uArizYZy24Gwtto51PB9hPyG+CSlO89w==" });
        }
    }
}
