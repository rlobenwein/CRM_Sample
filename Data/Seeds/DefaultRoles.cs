using Microsoft.AspNetCore.Identity;
using CRM_Sample.Data.Constants;
using System.Threading.Tasks;

namespace CRM_Sample.Data.Seeds
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
