using DryIoc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;

namespace RightVisionBotDb.Data.Configurations
{
    internal class RvUserEntityTypeConfiguration : IEntityTypeConfiguration<RvUser>
    {
        public void Configure(EntityTypeBuilder<RvUser> builder)
        {
            var locationService = App.Container.Resolve<LocationService>();

            //Enums
            builder.Property(u => u.Lang)
                .HasConversion<string>();

            builder.Property(u => u.Status)
                .HasConversion<string>();

            builder.Property(u => u.Role)
                .HasConversion<string>();

            //Types
            builder.Property(u => u.UserPermissions)
                .HasConversion(
                    v => ConvertHelper.PermissionsToString(v),
                    v => ConvertHelper.StringToPermissions(v)
                );
            builder.Property(u => u.Rewards)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<Reward>>(v)!
                );
            builder.Property(u => u.Punishments)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<RvPunishment>>(v)!
                );
            builder.Property(u => u.Location)
                .HasConversion(
                v => locationService.LocationToString(v),
                v => locationService.StringToLocation(v)
                );
        }
    }
}
