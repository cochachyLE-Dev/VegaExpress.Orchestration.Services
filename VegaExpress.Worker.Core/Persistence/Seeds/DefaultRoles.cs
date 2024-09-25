using Microsoft.AspNetCore.Identity;

using VegaExpress.Worker.Core.Persistence.Enums;

namespace VegaExpress.Worker.Core.Persistence.Seeds
{
    public static class DefaultRoles
    {
        public static List<IdentityRole> IdentityRoleList()
        {
            return new List<IdentityRole>()
            {
                new IdentityRole
                {
                    Id = Enums.Constants.SuperAdmin,
                    Name = Roles.SuperAdmin.ToString(),
                    NormalizedName = Roles.SuperAdmin.ToString()
                },
                new IdentityRole
                {
                    Id = Enums.Constants.Admin,
                    Name = Roles.Admin.ToString(),
                    NormalizedName = Roles.Admin.ToString()
                },
                new IdentityRole
                {
                    Id = Enums.Constants.Moderator,
                    Name = Roles.Moderator.ToString(),
                    NormalizedName = Roles.Moderator.ToString()
                },
                new IdentityRole
                {
                    Id = Enums.Constants.Basic,
                    Name = Roles.Basic.ToString(),
                    NormalizedName = Roles.Basic.ToString()
                }
            };
        }
    }
}
