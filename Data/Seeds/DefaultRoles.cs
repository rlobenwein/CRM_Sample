using Microsoft.AspNetCore.Identity;
using RLBW_ERP.Data.Constants;
using System.Threading.Tasks;

namespace RLBW_ERP.Data.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Director.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Commercial.ToString()));
        }
    }
}
