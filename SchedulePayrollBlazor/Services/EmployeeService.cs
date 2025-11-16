using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Utilities;

namespace SchedulePayrollBlazor.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _db;
    public EmployeeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Employee>> GetAllAsync()
    {
        return await _db.Employees
            .Include(e => e.User)
            .OrderBy(e => e.FullName)
            .ToListAsync();
    }

    public Task<Employee?> GetByIdAsync(int id)
    {
        return _db.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);
    }

    public async Task UpdateAsync(Employee employee, string? newPassword = null)
    {
        ArgumentNullException.ThrowIfNull(employee);

        if (_db.Entry(employee).State == EntityState.Detached)
        {
            _db.Employees.Attach(employee);
        }

        employee.FullName = employee.FullName?.Trim() ?? string.Empty;
        employee.Department = string.IsNullOrWhiteSpace(employee.Department)
            ? string.Empty
            : employee.Department.Trim();
        employee.JobTitle = string.IsNullOrWhiteSpace(employee.JobTitle)
            ? string.Empty
            : employee.JobTitle.Trim();
        employee.Location = string.IsNullOrWhiteSpace(employee.Location)
            ? string.Empty
            : employee.Location.Trim();

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            if (employee.User is null)
            {
                employee.User = await _db.Users.FirstOrDefaultAsync(u => u.UserId == employee.UserId)
                    ?? throw new InvalidOperationException("Unable to find associated user for employee.");
            }

            employee.User.PasswordHash = PasswordHasher.HashPassword(newPassword);
        }

        await _db.SaveChangesAsync();
    }
}
