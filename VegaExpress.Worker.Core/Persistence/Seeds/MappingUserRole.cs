using Microsoft.AspNetCore.Identity;

namespace VegaExpress.Worker.Core.Persistence.Seeds
{
    public static class MappingUserRole
    {
        public static List<IdentityUserRole<string>> IdentityUserRoleList()
        {
            return new List<IdentityUserRole<string>>()
            {
                new IdentityUserRole<string>
                {
                    RoleId = Enums.Constants.Basic,
                    UserId = Enums.Constants.BasicUser
                },
                new IdentityUserRole<string>
                {
                    RoleId = Enums.Constants.SuperAdmin,
                    UserId = Enums.Constants.SuperAdminUser
                },
                new IdentityUserRole<string>
                {
                    RoleId = Enums.Constants.Admin,
                    UserId = Enums.Constants.SuperAdminUser
                },
                new IdentityUserRole<string>
                {
                    RoleId = Enums.Constants.Moderator,
                    UserId = Enums.Constants.SuperAdminUser
                },
                new IdentityUserRole<string>
                {
                    RoleId = Enums.Constants.Basic,
                    UserId = Enums.Constants.SuperAdminUser
                }
            };
        }
    }
}
