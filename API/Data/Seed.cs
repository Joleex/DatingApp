using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class Seed
    {
         public static async Task SeedUsers(UserManager<AppUser> userManager,
         RoleManager<AppRole> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;

            var userData = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);
            if (users == null) return;

            var roles=new List<AppRole>
            {
                new AppRole{Name="Member"},
                new AppRole{Name="Admin"},
                new AppRole{Name="Moderator"}
            };

            foreach(var role in roles){
                await roleManager.CreateAsync(role);
            }

            foreach (var user in users)
            {

                user.UserName = user.UserName.ToLower();
                /*I'm not going to check the result of this.
                And if we don't get the right result, then we're going to do something because we only run this method
                once and only when our database is clean.
                And we're going to physically check to make sure that the data is inside there before we move forward.
                It's not something that our users is interacting with, so I'm skipping the checking of the results
                for these particular methods.*/
                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "Member");
            }

            //await context.SaveChangesAsync();

            var admin=new AppUser
            {
                UserName="admin"
            };
            await userManager.CreateAsync(admin, "Pa$$w0rd");
            await userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});
        }
    }
}