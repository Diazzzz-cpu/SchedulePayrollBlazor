using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;
using BCrypt.Net;

namespace SchedulePayrollBlazor.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly SimpleAuthStateProvider _authStateProvider;

    public AuthService(AppDbContext db, SimpleAuthStateProvider authStateProvider)
    {
        _db = db;
        _authStateProvider = authStateProvider;
    }

    public async Task<(bool Success, string Message)> RegisterAsync(
        string name, 
        string email, 
        string password, 
        int roleId,
        string employmentClass = "FullTime",
        string employmentType = "Hourly",
        decimal hourlyRate = 15.00m,
        decimal monthlyRate = 0m)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "All fields are required.");
        }

        if (await _db.Employees.AnyAsync(e => e.Email == email))
        {
            return (false, "Email already exists.");
        }

        var role = await _db.Roles.FindAsync(roleId);
        if (role == null)
        {
            return (false, "Invalid role selected.");
        }

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var employee = new Employee
        {
            Name = name,
            Email = email,
            Password = hashedPassword,
            RoleId = roleId,
            EmploymentClass = employmentClass,
            EmploymentType = employmentType,
            HourlyRate = hourlyRate,
            MonthlyRate = monthlyRate,
            Active = true
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        return (true, "Registration successful!");
    }

    public async Task<(bool Success, string Message, Employee? Employee)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Email and password are required.", null);
        }

        var employee = await _db.Employees
            .Include(e => e.Role)
            .FirstOrDefaultAsync(e => e.Email == email);

        if (employee == null)
        {
            return (false, "Invalid email or password.", null);
        }

        if (!BCrypt.Net.BCrypt.Verify(password, employee.Password))
        {
            return (false, "Invalid email or password.", null);
        }

        if (employee.Active != true)
        {
            return (false, "Account is inactive. Please contact administrator.", null);
        }

        await _authStateProvider.SignIn(email);

        return (true, "Login successful!", employee);
    }

    public async Task<List<Role>> GetRolesAsync()
    {
        return await _db.Roles.OrderBy(r => r.Name).ToListAsync();
    }
}
