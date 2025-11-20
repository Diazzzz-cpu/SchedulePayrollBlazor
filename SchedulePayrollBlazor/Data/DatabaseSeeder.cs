using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Utilities;   // <-- IMPORTANT: for PasswordHasher

namespace SchedulePayrollBlazor.Data;

public static class DatabaseSeeder
{
    private const string DefaultAdminEmail = "admin@example.com";
    private const string DefaultAdminPassword = "Admin123!";

    public static async Task EnsureSeedDataAsync(AppDbContext db)
    {
        // Make sure the DB & schema exist
        await db.Database.MigrateAsync();

        // 1) Seed roles (use names that match [Authorize(Roles = "Admin")])
        if (!await db.Roles.AnyAsync())
        {
            db.Roles.AddRange(
                new Role { Code = "ADMIN", Name = "Admin" },
                new Role { Code = "EMP", Name = "Employee" }
            );
            await db.SaveChangesAsync();
        }

        var adminRole = await db.Roles.FirstAsync(r => r.Code == "ADMIN");

        // 2) Seed admin user if missing
        var adminUser = await db.Users
            .FirstOrDefaultAsync(u => u.Email == DefaultAdminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                Email = DefaultAdminEmail,
                // HASH the password so AuthService can verify it
                PasswordHash = PasswordHasher.HashPassword(DefaultAdminPassword),
                FirstName = "System",
                LastName = "Admin",
                RoleId = adminRole.RoleId,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(adminUser);
            await db.SaveChangesAsync();
        }

        if (!await db.AttendancePenaltySettings.AnyAsync())
        {
            db.AttendancePenaltySettings.Add(new AttendancePenaltySettings
            {
                LatePenaltyPerMinute = 0m,
                UndertimePenaltyPerMinute = 0m,
                AbsenceFullDayMultiplier = 0m,
                OvertimeBonusPerMinute = 0m,
                UpdatedAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
    }
}
