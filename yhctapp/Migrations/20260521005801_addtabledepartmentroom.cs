using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace yhctapp.Migrations
{
    /// <inheritdoc />
    public partial class addtabledepartmentroom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "IdDepartmentRoom",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "DepartmentRooms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Room = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentRooms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_IdDepartmentRoom",
                table: "AspNetUsers",
                column: "IdDepartmentRoom");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_DepartmentRooms_IdDepartmentRoom",
                table: "AspNetUsers",
                column: "IdDepartmentRoom",
                principalTable: "DepartmentRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_DepartmentRooms_IdDepartmentRoom",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "DepartmentRooms");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_IdDepartmentRoom",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IdDepartmentRoom",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
