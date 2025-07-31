using App.DAL.EF;
using App.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Infrastructure.Data;

/// <summary>
/// DTO for serialization from config
/// </summary>
public class SeedUserSettings
{
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string[] Roles { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Primary interactions with a database
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seeding admin user
    /// </summary>
    /// <param name="app"></param>
    public static void Seed(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices
            .GetRequiredService<IServiceScopeFactory>()
            .CreateScope();

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        // 1) Migrations
        if (!context.Database.ProviderName!.Contains("InMemory"))
        {
            context.Database.Migrate();
        }

        // 2) Reading the list of users from the config
        var usersSection = config.GetSection("SeedData:Users");
        var seedUsers = usersSection.Get<List<SeedUserSettings>>()
                        ?? new List<SeedUserSettings>();

        foreach (var u in seedUsers)
        {
            // 2.1) create roles if they don't exist
            foreach (var role in u.Roles)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    roleManager.CreateAsync(new AppRole { Name = role }).Wait();
                }
            }

            // 2.2) check if there is already a user
            var existing = userManager.FindByEmailAsync(u.Email).Result;
            if (existing != null) continue;

            // 2.3) creating user
            var user = new AppUser
            {
                Email = u.Email,
                UserName = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName
            };
            var createRes = userManager.CreateAsync(user, u.Password).Result;
            if (!createRes.Succeeded)
            {
                Console.WriteLine($"Error creating {u.Email}: {string.Join(", ", createRes.Errors)}");
                continue;
            }

            // 2.4) add to roles
            var addRoleRes = userManager.AddToRolesAsync(user, u.Roles).Result;
            if (!addRoleRes.Succeeded)
            {
                Console.WriteLine($"Error assigning roles to {u.Email}: {string.Join(", ", addRoleRes.Errors)}");
            }
        }
    }
}