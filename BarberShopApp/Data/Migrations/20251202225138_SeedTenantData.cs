using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShopApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedTenantData : Migration
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

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "TenantId", "AdminEmail", "Name" },
                values: new object[] { 1, "admin@barberiacentral.com", "Barbería Central" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "TenantId",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Barbers");
        }
    }
}
