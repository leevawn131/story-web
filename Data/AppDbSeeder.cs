using Microsoft.EntityFrameworkCore;
using story_web.Models;

namespace story_web.Data;

public static class AppDbSeeder
{
    private const string DefaultAdminUserName = "admin";
    private const string DefaultAdminEmail = "admin@gmail.com";
    private const string DefaultAdminPassword = "123456";

    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("AppDbSeeder");

        try
        {
            if (!await context.Database.CanConnectAsync())
            {
                logger.LogWarning("Skipping admin seed because the database connection is not available.");
                return;
            }

            var admin = await context.Users
                .FirstOrDefaultAsync(user =>
                    user.UserName.ToLower() == DefaultAdminUserName ||
                    user.Email.ToLower() == DefaultAdminEmail);

            if (admin is null)
            {
                admin = new User
                {
                    UserName = DefaultAdminUserName,
                    Email = DefaultAdminEmail,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultAdminPassword),
                    Role = UserRoles.Admin,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };

                context.Users.Add(admin);
            }
            else
            {
                admin.UserName = DefaultAdminUserName;
                admin.Email = DefaultAdminEmail;
                admin.Role = UserRoles.Admin;
                admin.ModifiedAt = DateTime.UtcNow;

                if (string.IsNullOrWhiteSpace(admin.PasswordHash) ||
                    !BCrypt.Net.BCrypt.Verify(DefaultAdminPassword, admin.PasswordHash))
                {
                    admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultAdminPassword);
                }
            }

            await context.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Skipping admin seed because the database is unavailable or the Users schema does not match the expected model.");
        }
    }
}
