using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace yhctapp.Migrations
{
    /// <inheritdoc />
    public partial class documenttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_DepartmentRoom = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Thumnail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentGroups_DepartmentRooms_Id_DepartmentRoom",
                        column: x => x.Id_DepartmentRoom,
                        principalTable: "DepartmentRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentGroups_Id_DepartmentRoom",
                table: "DocumentGroups",
                column: "Id_DepartmentRoom");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentGroups");
        }
    }
}
