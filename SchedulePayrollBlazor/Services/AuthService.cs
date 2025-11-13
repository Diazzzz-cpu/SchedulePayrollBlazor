using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Utilities;

namespace SchedulePayrollBlazor.Services;

public class AuthService
{
    private readonly AppDbContext _dbContext;
    private readonly SimpleAuthStateProvider _authStateProvider;

    // adjust these if your role_id mapping is different
    private const int AdminRoleId = 5;     // ADMIN row in role table

    public AuthService(AppDbContext dbContext, SimpleAuthStateProvider authStateProvider)
    {
        _dbContext = dbContext;
        _authStateProvider = authStateProvider;
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
