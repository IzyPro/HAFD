using Microsoft.EntityFrameworkCore.Migrations;

namespace HAFD.Migrations
{
    public partial class SecondMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hostels_AspNetUsers_OccupantId",
                table: "Hostels");

            migrationBuilder.DropIndex(
                name: "IX_Hostels_OccupantId",
                table: "Hostels");

            migrationBuilder.DropColumn(
                name: "OccupantId",
                table: "Hostels");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Hostels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HostelId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_HostelId",
                table: "AspNetUsers",
                column: "HostelId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Hostels_HostelId",
                table: "AspNetUsers",
                column: "HostelId",
                principalTable: "Hostels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Hostels_HostelId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_HostelId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Hostels");

            migrationBuilder.DropColumn(
                name: "HostelId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "OccupantId",
                table: "Hostels",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hostels_OccupantId",
                table: "Hostels",
                column: "OccupantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hostels_AspNetUsers_OccupantId",
                table: "Hostels",
                column: "OccupantId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
