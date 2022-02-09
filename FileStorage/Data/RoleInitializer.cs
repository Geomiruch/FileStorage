using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileStorage.Data
{
    public class RoleInitializer
    {
        public static async Task InitializeAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string adminEmail = "admin@gmail.com";
            string adminName = "admin";
            string moderatorEmail = "moderator@gmail.com";
            string moderatorName = "moderator";
            string password = "_Aa123456";
            if (await roleManager.FindByNameAsync("admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }
            if (await roleManager.FindByNameAsync("moderator") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("moderator"));
            }
            

            if (await userManager.FindByNameAsync(adminName) == null)
            {
                IdentityUser admin = new IdentityUser { Email = adminEmail, UserName = adminName, EmailConfirmed=true };
                IdentityResult result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }
            if (await userManager.FindByNameAsync(moderatorName) == null)
            {
                IdentityUser moderator = new IdentityUser { Email = moderatorEmail, UserName = moderatorName, EmailConfirmed = true };
                IdentityResult result = await userManager.CreateAsync(moderator, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(moderator, "moderator");
                }
            }
        }
    }
}
