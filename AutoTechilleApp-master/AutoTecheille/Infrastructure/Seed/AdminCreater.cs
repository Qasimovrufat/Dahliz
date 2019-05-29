using AutoTecheille.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTecheille.Models.MyClass
{
    public static class AdminCreater
    {
        public async static Task CreatAsync(IServiceScope service,AutoEntity db)
        {
            var userManager = service.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = service.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if(!db.Users.Any() && !db.UserRoles.Any())
            {
                var user = new User()
                {
                    UserName = "admin",
                    Email = "auto@gmail.com"
                };

                var identityResult = await userManager.CreateAsync(user, "Admin123@");
                if (identityResult.Succeeded)
                {
                    await roleManager.CreateAsync(new IdentityRole() { Name = "Admin"});
                    var roleResult = await userManager.AddToRoleAsync(user, "Admin");
                    if (roleResult.Succeeded)
                    {
                        await db.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
