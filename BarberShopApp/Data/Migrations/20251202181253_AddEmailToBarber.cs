using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShopApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailToBarber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Barbers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Barbers");
        }
    }
}
