using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShopApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneToBarber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Barbers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Barbers");
        }
    }
}
