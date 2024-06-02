using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Recipes.Data.Models;
using static System.Formats.Asn1.AsnWriter;

namespace Recipes.DAL.Seeding
{
    public class RolesUsersSeeding
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {

            var roleManager = serviceProvider.GetRequiredService<RoleManager<UserRole>>();
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(
                    new UserRole { Id = Guid.NewGuid(), Name = "Administrator", NormalizedName = "ADMINISTRATOR".ToUpper() });
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(
                    new UserRole { Id = Guid.NewGuid(), Name = "User", NormalizedName = "USER".ToUpper() });
            }
        }

        public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            // Seed Admin User
            var adminUser = new User
            {
                Id = Guid.Parse("8e445865-a24d-4543-a6c6-9443d048cdb9"),
                UserName = "adminUser@example.com",
                Email = "adminUser@example.com",
                NormalizedUserName = "ADMINUSER",
                FirstName="Popka",
                LastName="Negr",
                Avatar= "https://images.unsplash.com/photo-1587064712555-6e206484699b?q=80&w=1000&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTl8fGJsYWNrJTIwbWFufGVufDB8fDB8fHww"
            };

            if (userManager.Users.All(u => u.UserName != adminUser.UserName))
            {
                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    // Assign the "Admin" role to the admin user
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }
            }

            // Seed Standard User
            var standardUser = new User
            {
                Id = Guid.Parse("9c445865-a24d-4233-a6c6-9443d048cdb9"),
                UserName = "user@example.com",
                Email = "user@example.com",
                NormalizedUserName = "USER",
                FirstName = "Black",
                LastName = "Nicker",
                Avatar = "https://images.unsplash.com/photo-1587064712555-6e206484699b?q=80&w=1000&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxzZWFyY2h8MTl8fGJsYWNrJTIwbWFufGVufDB8fDB8fHww"
            };

            if (userManager.Users.All(u => u.UserName != standardUser.UserName))
            {
                var result = await userManager.CreateAsync(standardUser, "User123!");
                if (result.Succeeded)
                {
                    // Assign the "User" role to the standard user
                    await userManager.AddToRoleAsync(standardUser, "User");
                }
            }
        }
    }
}
