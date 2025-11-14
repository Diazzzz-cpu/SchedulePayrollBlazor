        using System;
        using System.Threading.Tasks;
        using Microsoft.EntityFrameworkCore;
        using SchedulePayrollBlazor.Data.Models;
        using SchedulePayrollBlazor.Utilities;

        namespace SchedulePayrollBlazor.Data;

        public static class DatabaseSeeder
        {
            // IMPORTANT: this must match an existing row in your `role` table.
            // If your Admin role has a different ID, change 5 to that value.
            private const int AdminRoleId = 5;

            // Seed credentials:
            // Email:    admin@system.com
            // Password: Admin123!
            private const string DefaultAdminEmail = "admin@system.com";
            private const string DefaultAdminPassword = "Admin123!";

            public static async Task EnsureSeedDataAsync(AppDbContext db)
            {
                // We only ensure the Admin user exists – we do NOT touch the `role` table.
                // Remove migrations here – your schema already exists.
                // await db.Database.MigrateAsync();   // <- comment out / remove

                var adminUser = await db.Users
                  .FirstOrDefaultAsync(u => u.Email == DefaultAdminEmail);

                if (adminUser is null)
                {
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
                    adminUser.RoleId = AdminRoleId;
                    adminUser.FirstName = "System";
                    adminUser.LastName = "Administrator";
                    adminUser.PasswordHash = PasswordHasher.HashPassword(DefaultAdminPassword);
                }

                await db.SaveChangesAsync();
            }
        }
