using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberShopApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiTenancyColumnsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Barbers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    TenantId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AdminEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.TenantId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_TenantId",
                table: "Services",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Barbers_TenantId",
                table: "Barbers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TenantId",
                table: "Appointments",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Tenants_TenantId",
                table: "Appointments",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Barbers_Tenants_TenantId",
                table: "Barbers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Tenants_TenantId",
                table: "Services",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "TenantId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Tenants_TenantId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Barbers_Tenants_TenantId",
                table: "Barbers");

            migrationBuilder.DropForeignKey(
                name: "FK_Services_Tenants_TenantId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Services_TenantId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Barbers_TenantId",
                table: "Barbers");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_TenantId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Barbers");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Appointments");
        }
    }
}
