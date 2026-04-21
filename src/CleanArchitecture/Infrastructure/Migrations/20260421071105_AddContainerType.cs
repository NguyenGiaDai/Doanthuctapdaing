using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContainerType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContainerTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContainerTypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContainerTypeName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ISOCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ContainerSize = table.Column<int>(type: "int", nullable: false),
                    MaximumWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TareWeight = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContainerTypes", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "82607d96-7d29-469b-b3d6-f1a765accdcd", "AQAAAAIAAYagAAAAEHQ22ftG2HXIu1AYOzqt5RORVnETSuUif6uArizYZy24Gwtto51PB9hPyG+CSlO89w==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContainerTypes");

            migrationBuilder.UpdateData(
                table: "ApplicationUser",
                keyColumn: "Id",
                keyValue: new Guid("69db714f-9576-45ba-b5b7-f00649be01de"),
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "3ceb2e15-1cf9-45aa-94bc-4f86497d7cbc", "AQAAAAIAAYagAAAAEPu3PoZ6JR0Q9Sh2B0kTxcKqy2TOA9SrzmCuGTxw4MaTRDZrCFyOvxKKWjKhrhnisg==" });
        }
    }
}
