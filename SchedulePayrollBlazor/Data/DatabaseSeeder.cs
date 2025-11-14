using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Utilities;

namespace SchedulePayrollBlazor.Data
{
    public static class DatabaseSeeder
    {
        private const int AdminRoleId = 5;                      // role_id for ADMIN in your role table
        private const string DefaultAdminEmail = "admin@system.com";
        private const string DefaultAdminPassword = "Admin123!";

        public static async Task EnsureSeedDataAsync(AppDbContext db)
        {
            // Apply migrations if any (or just ensure DB is reachable)
            await db.Database.MigrateAsync();

            // Find the admin user by email
            var adminUser = await db.Users
                .FirstOrDefaultAsync(u => u.Email == DefaultAdminEmail);

            if (adminUser == null)
            {
                // Create a brand-new admin user
                adminUser = new User
                {
                    Email = DefaultAdminEmail,
                    FirstName = "System",
                    LastName = "Administrator",
                    RoleId = AdminRoleId,
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = PasswordHasher.HashPassword(DefaultAdminPassword)
                };

                db.Users.Add(adminUser);
            }
            else
            {
                // Ensure it really is an admin and reset the password
                adminUser.RoleId = AdminRoleId;
                adminUser.FirstName = "System";
                adminUser.LastName = "Administrator";
                adminUser.PasswordHash = PasswordHasher.HashPassword(DefaultAdminPassword);
            }

            // 👉 No Employee creation here – we avoid touching the employee table entirely
            await db.SaveChangesAsync();
        }
    }
}