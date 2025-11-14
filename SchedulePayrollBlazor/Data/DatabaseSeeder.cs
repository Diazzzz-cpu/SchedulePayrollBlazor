using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Utilities;

namespace SchedulePayrollBlazor.Data;

public static class DatabaseSeeder
{
    private const string AdminRoleName = "Admin";
    private const string EmployeeRoleName = "Employee";

    // Seed credentials:
    // Email: admin@system.com
    // Password: Admin123!
    private const string DefaultAdminEmail = "admin@system.com";
    private const string DefaultAdminPassword = "Admin123!";

    public static async Task EnsureSeedDataAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        var adminRole = await EnsureRoleAsync(db, AdminRoleName);
        await EnsureRoleAsync(db, EmployeeRoleName);

        var adminUser = await db.Users
            .FirstOrDefaultAsync(u => u.Email == DefaultAdminEmail);

        if (adminUser is null)
        {
            adminUser = new User
            {
                Email = DefaultAdminEmail,
                FirstName = "System",
                LastName = "Administrator",
                RoleId = adminRole.RoleId,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = PasswordHasher.HashPassword(DefaultAdminPassword)
            };

            db.Users.Add(adminUser);
        }
        else
        {
            adminUser.RoleId = adminRole.RoleId;
            adminUser.FirstName = "System";
            adminUser.LastName = "Administrator";
            adminUser.PasswordHash = PasswordHasher.HashPassword(DefaultAdminPassword);
        }

        await db.SaveChangesAsync();
    }

    private static async Task<Role> EnsureRoleAsync(AppDbContext db, string roleName)
    {
        var normalized = roleName.Trim();
        var lower = normalized.ToLowerInvariant();

        var role = await db.Roles
            .FirstOrDefaultAsync(r => r.RoleName == normalized || r.RoleName.ToLower() == lower);

        if (role is not null)
        {
            return role;
        }

        role = new Role
        {
            RoleName = normalized
        };

        db.Roles.Add(role);
        await db.SaveChangesAsync();

        return role;
    }
}
