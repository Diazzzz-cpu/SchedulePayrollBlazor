using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Services.Models;
using SchedulePayrollBlazor.Utilities;

namespace SchedulePayrollBlazor.Services;

public class AdminUserService
{
    private const int AdminRoleId = 5;
    private const int DefaultEmployeeRoleId = 1;

    private readonly AppDbContext _dbContext;

    public AdminUserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AdminUserSummary>> GetUsersAsync()
    {
        return await _dbContext.Users
            .Include(u => u.Employee)
            .OrderBy(u => u.Email)
            .Select(u => new AdminUserSummary
            {
                UserId = u.UserId,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                RoleId = u.RoleId,
                CreatedAt = u.CreatedAt,
                Department = u.Employee != null ? u.Employee.Department : null,
                JobTitle = u.Employee != null ? u.Employee.JobTitle : null,
                EmploymentType = u.Employee != null ? u.Employee.EmploymentType : null,
                StartDate = u.Employee != null ? u.Employee.StartDate : null,
                Location = u.Employee != null ? u.Employee.Location : null
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string? ErrorMessage, string? GeneratedPassword)> CreateUserAsync(AdminUserFormModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var email = model.Email.Trim().ToLowerInvariant();
        if (await _dbContext.Users.AnyAsync(u => u.Email == email))
        {
            return (false, "A user with this email already exists.", null);
        }

        var roleId = ResolveRoleId(model.Role);
        var password = string.IsNullOrWhiteSpace(model.Password)
            ? null
            : model.Password.Trim();
        string? generatedPassword = null;

        if (string.IsNullOrWhiteSpace(password))
        {
            generatedPassword = GenerateTemporaryPassword();
            password = generatedPassword;
        }

        var user = new User
        {
            Email = email,
            PasswordHash = PasswordHasher.HashPassword(password),
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            RoleId = roleId,
            CreatedAt = DateTime.UtcNow
        };

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            if (ShouldCreateEmployee(model))
            {
                var employee = new Employee
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Department = model.Department?.Trim() ?? string.Empty,
                    JobTitle = model.JobTitle?.Trim() ?? string.Empty,
                    EmploymentType = string.IsNullOrWhiteSpace(model.EmploymentType)
                        ? "FullTime"
                        : model.EmploymentType.Trim(),
                    StartDate = model.StartDate ?? DateTime.UtcNow,
                    Location = model.Location?.Trim() ?? string.Empty,
                    IsActive = true
                };

                _dbContext.Employees.Add(employee);
                await _dbContext.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return (true, null, generatedPassword);
        }
        catch
        {
            await transaction.RollbackAsync();
            return (false, "Failed to create the user. Please try again.", null);
        }
    }

    public async Task<(bool Success, string? ErrorMessage, string? GeneratedPassword)> UpdateUserAsync(AdminUserFormModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        if (model.UserId is null)
        {
            return (false, "Missing user identifier.", null);
        }

        var email = model.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.UserId == model.UserId.Value);

        if (user is null)
        {
            return (false, "User not found.", null);
        }

        var emailExists = await _dbContext.Users
            .AnyAsync(u => u.Email == email && u.UserId != user.UserId);

        if (emailExists)
        {
            return (false, "Another user with this email already exists.", null);
        }

        var roleId = ResolveRoleId(model.Role);

        user.Email = email;
        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.RoleId = roleId;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = PasswordHasher.HashPassword(model.Password.Trim());
        }

        var shouldHaveEmployee = ShouldCreateEmployee(model);

        if (shouldHaveEmployee)
        {
            if (user.Employee is null)
            {
                user.Employee = new Employee
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Department = model.Department?.Trim() ?? string.Empty,
                    JobTitle = model.JobTitle?.Trim() ?? string.Empty,
                    EmploymentType = string.IsNullOrWhiteSpace(model.EmploymentType)
                        ? "FullTime"
                        : model.EmploymentType.Trim(),
                    StartDate = model.StartDate ?? DateTime.UtcNow,
                    Location = model.Location?.Trim() ?? string.Empty,
                    IsActive = true
                };
            }
            else
            {
                user.Employee.FirstName = user.FirstName;
                user.Employee.LastName = user.LastName;
                user.Employee.Department = model.Department?.Trim() ?? string.Empty;
                user.Employee.JobTitle = model.JobTitle?.Trim() ?? string.Empty;
                user.Employee.EmploymentType = string.IsNullOrWhiteSpace(model.EmploymentType)
                    ? user.Employee.EmploymentType
                    : model.EmploymentType.Trim();
                user.Employee.StartDate = model.StartDate ?? user.Employee.StartDate;
                user.Employee.Location = model.Location?.Trim() ?? string.Empty;
            }
        }
        else if (user.Employee is not null)
        {
            _dbContext.Employees.Remove(user.Employee);
        }

        await _dbContext.SaveChangesAsync();
        return (true, null, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(int userId, int currentUserId)
    {
        if (userId == currentUserId)
        {
            return (false, "You cannot delete your own account while signed in.");
        }

        var user = await _dbContext.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null)
        {
            return (false, "User not found.");
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        return (true, null);
    }

    private static int ResolveRoleId(string role)
    {
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
            ? AdminRoleId
            : DefaultEmployeeRoleId;
    }

    private static bool ShouldCreateEmployee(AdminUserFormModel model)
    {
        return model.IsEmployeeRole
               || !string.IsNullOrWhiteSpace(model.Department)
               || !string.IsNullOrWhiteSpace(model.JobTitle)
               || model.StartDate.HasValue
               || !string.IsNullOrWhiteSpace(model.Location);
    }

    private static string GenerateTemporaryPassword()
    {
        return Guid.NewGuid().ToString("N")[..10];
    }
}
