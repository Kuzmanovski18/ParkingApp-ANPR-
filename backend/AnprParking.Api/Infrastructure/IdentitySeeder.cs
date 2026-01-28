using AnprParking.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace AnprParking.Api.Infrastructure;

public static class IdentitySeeder
{
    public static async Task SeedAdminAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();

        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var adminUsername = cfg["Seed:AdminUsername"] ?? "admin";
        var adminPassword = cfg["Seed:AdminPassword"] ?? "Admin123!";
        var adminEmail = cfg["Seed:AdminEmail"] ?? "admin@anpr.local";

        // 1) Role "Admin"
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // 2) User
        var user = await userManager.FindByNameAsync(adminUsername);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = adminUsername,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var createRes = await userManager.CreateAsync(user, adminPassword);
            if (!createRes.Succeeded)
            {
                var msg = string.Join("; ", createRes.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new Exception("Admin seed failed: " + msg);
            }
        }

        // 3) Add to role
        if (!await userManager.IsInRoleAsync(user, "Admin"))
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}
