using System;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Services.Models;
using SchedulePayrollBlazor.Utilities;

namespace SchedulePayrollBlazor.Services;

public class AuthService
{
    private readonly AppDbContext _dbContext;
    private readonly SimpleAuthStateProvider _authStateProvider;

    // adjust these if your role_id mapping is different
    private const int AdminRoleId = 5;     // ADMIN row in role table
    private const int DefaultEmployeeRoleId = 1; // TEACHER or generic employee

    public AuthService(AppDbContext dbContext, SimpleAuthStateProvider authStateProvider)
    {
        _dbContext = dbContext;
        _authStateProvider = authStateProvider;
    }

    public async Task<(bool Success, string ErrorMessage)> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _dbContext.Users.AnyAsync(u => u.Email == email))
        {
            return (false, "Email already exists.");
        }

        // map string Role ("Admin" / etc.) to numeric role_id
        int roleId = string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase)
            ? AdminRoleId
            : DefaultEmployeeRoleId;

        await using var tx = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var user = new User
            {
                Email = email,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            // Optional Employee record
            var employee = new Employee
            {
                UserId = user.UserId,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                JobTitle = request.JobTitle ?? string.Empty,
                Department = request.Department ?? string.Empty,
                EmploymentType = request.EmploymentType ?? "FullTime",
                StartDate = request.StartDate,
                Location = request.Location ?? string.Empty,
                IsActive = true
            };

            _dbContext.Employees.Add(employee);
            await _dbContext.SaveChangesAsync();

            await tx.CommitAsync();

            await _authStateProvider.SignInAsync(user.UserId);

            return (true, string.Empty);
        }
        catch
        {
            await tx.RollbackAsync();
            return (false, "Registration failed.");
        }
    }

    // return Role as a string so Login.razor can use it directly
    public async Task<(bool Success, string ErrorMessage, string Role)> LoginAsync(string email, string password)
    {
        email = email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            return (false, "Invalid email or password.", string.Empty);
        }

        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return (false, "Invalid email or password.", string.Empty);
        }

        await _authStateProvider.SignInAsync(user.UserId);

        // map numeric RoleId to a label for the UI
        string roleName = user.RoleId == AdminRoleId ? "Admin" : "Employee";

        return (true, string.Empty, roleName);
    }

    public Task LogoutAsync()
    {
        return _authStateProvider.SignOutAsync();
    }
}
