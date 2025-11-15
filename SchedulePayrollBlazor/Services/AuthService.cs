using System;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Utilities;

namespace SchedulePayrollBlazor.Services;

public class AuthService
{
    private readonly AppDbContext _dbContext;
    private readonly SimpleAuthStateProvider _authStateProvider;

    public AuthService(AppDbContext dbContext, SimpleAuthStateProvider authStateProvider)
    {
        _dbContext = dbContext;
        _authStateProvider = authStateProvider;
    }

    public async Task<(bool Success, string ErrorMessage, string Role)> LoginAsync(string email, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "Email and password are required.", string.Empty);
            }

            email = email.Trim().ToLowerInvariant();

            var user = await _dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null)
            {
                return (false, "Invalid email or password.", string.Empty);
            }

            if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                return (false, "Invalid email or password.", string.Empty);
            }

            if (user.Employee is not null && !user.Employee.IsActive)
            {
                return (false, "Invalid email or password.", string.Empty);
            }

            await _authStateProvider.SignInAsync(user.UserId);

            var roleName = NormalizeRoleName(user.Role?.Name);

            return (true, string.Empty, roleName);
        }
        catch (Exception ex)
        {
            return (false, $"Login service error: {ex.Message}", string.Empty);
        }
    }

    public Task LogoutAsync()
    {
        return _authStateProvider.SignOutAsync();
    }

    private static string NormalizeRoleName(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return "Employee";
        }

        if (roleName.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Admin";
        }

        if (roleName.Equals("employee", StringComparison.OrdinalIgnoreCase))
        {
            return "Employee";
        }

        // Default to the employee role for any other values to keep
        // authorization checks consistent across the app.
        return "Employee";
    }
}
