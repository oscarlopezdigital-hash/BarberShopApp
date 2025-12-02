// Data/Configurations/TenantConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BarberShopApp.Models;

namespace BarberShopApp.Data.Configurations
{
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasData(
                new Tenant
                {
                    TenantId = 1,
                    Name = "Barbería Central",
                    // *** ¡NUEVA LÍNEA REQUERIDA! ***
                    AdminEmail = "admin@barberiacentral.com"
                }
            );
        }
    }
}