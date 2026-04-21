using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorContainerToUseFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContainerSize",
                table: "Containers");

            migrationBuilder.DropColumn(
                name: "ContainerType",
                table: "Containers");

            migrationBuilder.DropColumn(
                name: "IsoCode",
                table: "Containers");

            migrationBuilder.DropColumn(
                name: "MaximumWeight",
                table: "Containers");

            migrationBuilder.DropColumn(
                name: "TareWeight",
                table: "Containers");

            migrationBuilder.AlterColumn<string>(
                name: "ContainerOwner",
                table: "Containers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ContainerNumber",
                table: "Containers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ContainerCondition",
                table: "Containers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ContainerClassification",
                table: "Containers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContainerTypeId",
                table: "Containers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentStatus",
                table: "Containers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineOperatorId",
                table: "Containers",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "44ec42d4-28f7-4042-9c49-a56242dbdb94", "AQAAAAIAAYagAAAAEDREj1YFjhHUeOfsCwNgZML95Ck20McvpcNnG7RzhxK1BZgFcRYEncHWsu+hyU+WmA==" });

            migrationBuilder.CreateIndex(
                name: "IX_Containers_ContainerTypeId",
                table: "Containers",
                column: "ContainerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Containers_LineOperatorId",
                table: "Containers",
                column: "LineOperatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Containers_ContainerTypes_ContainerTypeId",
                table: "Containers",
                column: "ContainerTypeId",
                principalTable: "ContainerTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Containers_LineOperators_LineOperatorId",
                table: "Containers",
                column: "LineOperatorId",
                principalTable: "LineOperators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Containers_ContainerTypes_ContainerTypeId",
                table: "Containers");

            migrationBuilder.DropForeignKey(
                name: "FK_Containers_LineOperators_LineOperatorId",
                table: "Containers");

            migrationBuilder.DropIndex(
                name: "IX_Containers_ContainerTypeId",
                table: "Containers");

            migrationBuilder.DropIndex(
                name: "IX_Containers_LineOperatorId",
                table: "Containers");

            migrationBuilder.DropColumn(
                name: "ContainerClassification",
                table: "Containers");

            migrationBuilder.DropColumn(
                name: "ContainerTypeId",
                table: "Containers");

            migrationBuilder.DropColumn(
                name: "CurrentStatus",
                table: "Containers");

            migrationBuilder.DropColumn(
                name: "LineOperatorId",
                table: "Containers");

            migrationBuilder.AlterColumn<string>(
                name: "ContainerOwner",
                table: "Containers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "ContainerNumber",
                table: "Containers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ContainerCondition",
                table: "Containers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "ContainerSize",
                table: "Containers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContainerType",
                table: "Containers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IsoCode",
                table: "Containers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumWeight",
                table: "Containers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TareWeight",
                table: "Containers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "34cf3477-bd6c-45e2-8ebb-d3995deca2fc", "AQAAAAIAAYagAAAAEC+sIUEGvaU6A8cLjSEos9vxJmLCgnAQLFCLCWAwbWeFAeM4gZndgXrSDGE5c5bdkw==" });
        }
    }
}
