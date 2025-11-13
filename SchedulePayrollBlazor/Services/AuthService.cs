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

    public AuthService(AppDbContext dbContext, SimpleAuthStateProvider authStateProvider)
    {
        _dbContext = dbContext;
        _authStateProvider = authStateProvider;
    }

    public async Task<(bool Success, string ErrorMessage)> RegisterAsync(RegisterRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var role = string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "Employee";
        var employmentType = string.IsNullOrWhiteSpace(request.EmploymentType)
            ? "FullTime"
            : request.EmploymentType.Trim();

        if (await _dbContext.Users.AnyAsync(u => u.Email == normalizedEmail))
        {
            return (false, "An account with that email already exists.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var user = new User
            {
                Email = normalizedEmail,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            var employee = new Employee
            {
                UserId = user.Id,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Department = (request.Department ?? string.Empty).Trim(),
                JobTitle = (request.JobTitle ?? string.Empty).Trim(),
                EmploymentType = employmentType,
                StartDate = request.StartDate,
                Location = (request.Location ?? string.Empty).Trim(),
                IsActive = true
            };

            _dbContext.Employees.Add(employee);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            await _authStateProvider.SignInAsync(user.Id);

            return (true, string.Empty);
        }
        catch
        {
            await transaction.RollbackAsync();
            return (false, "We couldn't complete your registration. Please try again.");
        }
    }

    public async Task<(bool Success, string ErrorMessage, string Role)> LoginAsync(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user is null)
        {
            return (false, "Invalid email or password.", string.Empty);
        }

        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return (false, "Invalid email or password.", string.Empty);
        }

        await _authStateProvider.SignInAsync(user.Id);

        return (true, string.Empty, user.Role);
    }

    public async Task LogoutAsync()
    {
        await _authStateProvider.SignOutAsync();
    }
}
