using VegaExpress.Worker.Core.Entities;

namespace VegaExpress.Worker.Core.Persistence.Seeds
{
    public static class DefaultUser
    {
        public static List<ApplicationUser> IdentityBasicUserList()
        {
            return new List<ApplicationUser>()
            {
                new ApplicationUser
                {
                    Id = Enums.Constants.SuperAdminUser,
                    UserName = "superadmin",
                    Email = "superadmin@vaetech.net",
                    FirstName = "super",
                    LastName = "admin",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    // Abc123.
                    PasswordHash = "AQAAAAEAACcQAAAAEILmMoVpxjCMsDvSVSoRmhNqgUfYl09gmYJGy4PTyJZ6kKnQaED01rIENgKBg1++SQ==",
                    NormalizedEmail= "SUPERADMIN@VAETECH.NET",
                    NormalizedUserName="SUPERADMIN"
                },
                new ApplicationUser
                {
                    Id = Enums.Constants.BasicUser,
                    UserName = "basicuser",
                    Email = "basicuser@vaetech.net",
                    FirstName = "Basic",
                    LastName = "User",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    // Abc123.
                    PasswordHash = "AQAAAAEAACcQAAAAEILmMoVpxjCMsDvSVSoRmhNqgUfYl09gmYJGy4PTyJZ6kKnQaED01rIENgKBg1++SQ==",
                    NormalizedEmail= "BASICUSER@VAETECH.NET",
                    NormalizedUserName="BASICUSER"
                },
            };
        }
    }
}
