using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskMan.Data;
using TaskMan.Models;

namespace TaskMan.Services
{
    public class SeedService
    {
        public static async System.Threading.Tasks.Task SeedDatabase(IServiceProvider serviceProvider)
        {
            // Create a scope to get the required services
            using var scope = serviceProvider.CreateScope();
            // Get the required services from the scope
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                // Ensure database is ready
                logger.LogInformation("Ensuring database is created...");
                await context.Database.MigrateAsync();

                // Seed roles
                logger.LogInformation("Seeding roles...");
                await AddRoleAsync(roleManager, "Admin");
                await AddRoleAsync(roleManager, "User");

                // Add admin user
                logger.LogInformation("Seeding admin user...");
                var adminEmail = "admin@default.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    var adminUser = new User
                    {
                        UserName = adminEmail,
                        NormalizedUserName = adminEmail.ToUpper(),
                        Email = adminEmail,
                        NormalizedEmail = adminEmail.ToUpper(),
                        EmailConfirmed = true,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        TwoFactorEnabled = true, 
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Admin user created successfully.");
                        // Assign admin role to the user
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                    else
                    {
                        logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));

                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async System.Threading.Tasks.Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            // Check if the role already exists before creating it
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                // Create the role if it doesn't exist
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

            }
        }
    }
}
