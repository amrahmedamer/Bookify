using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify2.Data.Migrations
{
    public partial class AddIndexOnMobileNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Subscripers_MobileNumber",
                table: "Subscripers",
                column: "MobileNumber",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscripers_MobileNumber",
                table: "Subscripers");
        }
    }
}
