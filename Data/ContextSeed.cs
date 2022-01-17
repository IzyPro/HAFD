using System;
using System.Threading.Tasks;
using HAFD.Enums;
using HAFD.Models;
using Microsoft.AspNetCore.Identity;

namespace HAFD.Data
{
    public static class ContextSeed
    {
        public static async Task SeedRolesAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            await roleManager.CreateAsync(new IdentityRole(UserRolesEnum.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(UserRolesEnum.User.ToString()));
        }
    }
}
