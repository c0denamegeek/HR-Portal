using HR_Portal.Constants;
using HR_Portal.Data;
using HR_Portal.Interfaces;
using HR_Portal.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HR_Portal.Services
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                logger.LogInformation("Applying pending migrations.");
                await context.Database.MigrateAsync();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                logger.LogInformation("Seeding roles.");
                await AddRoleAsync(roleManager, Roles.Admin);
                await AddRoleAsync(roleManager, Roles.User);

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
                var userAccountService = scope.ServiceProvider.GetRequiredService<IUserAccountService>();

                const string adminEmail = "alulutho.matoti@also-sa.com";
                if (await userManager.FindByEmailAsync(adminEmail) == null)
                {
                    logger.LogInformation("Seeding admin user.");
                    var result = await userAccountService.CreateUserAsync(
                        name: "Admin",
                        surname: "User",
                        email: adminEmail,
                        department: "HR",
                        password: "@dminALS0",
                        role: Roles.Admin);

                    if (!result.Succeeded)
                    {
                        logger.LogError("Failed to create Admin User: {Errors}",
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task AddRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}