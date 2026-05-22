using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace yhctapp.Migrations
{
    /// <inheritdoc />
    public partial class documentrecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Id_DepartmentRoom = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NamHieuLuc = table.Column<int>(type: "int", nullable: false),
                    ThoiHanLuuTru = table.Column<int>(type: "int", nullable: false),
                    ViTriLuuTru = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NguoiQuanLy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TinhTrang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MucDoBaoMat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Id_DocumentGroup = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentRecords_DepartmentRooms_Id_DepartmentRoom",
                        column: x => x.Id_DepartmentRoom,
                        principalTable: "DepartmentRooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentRecords_DocumentGroups_Id_DocumentGroup",
                        column: x => x.Id_DocumentGroup,
                        principalTable: "DocumentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRecords_Id_DepartmentRoom",
                table: "DocumentRecords",
                column: "Id_DepartmentRoom");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentRecords_Id_DocumentGroup",
                table: "DocumentRecords",
                column: "Id_DocumentGroup");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentRecords");
        }
    }
}
