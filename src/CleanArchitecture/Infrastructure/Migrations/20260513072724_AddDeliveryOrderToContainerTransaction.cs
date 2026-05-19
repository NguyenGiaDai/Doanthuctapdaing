using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryOrderToContainerTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliveryOrderId",
                table: "ContainerTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContainerTransactions_DeliveryOrderId",
                table: "ContainerTransactions",
                column: "DeliveryOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContainerTransactions_DeliveryOrders_DeliveryOrderId",
                table: "ContainerTransactions",
                column: "DeliveryOrderId",
                principalTable: "DeliveryOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContainerTransactions_DeliveryOrders_DeliveryOrderId",
                table: "ContainerTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ContainerTransactions_DeliveryOrderId",
                table: "ContainerTransactions");

            migrationBuilder.DropColumn(
                name: "DeliveryOrderId",
                table: "ContainerTransactions");
        }
    }
}
