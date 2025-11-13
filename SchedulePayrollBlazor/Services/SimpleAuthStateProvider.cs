using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SchedulePayrollBlazor.Data;

namespace SchedulePayrollBlazor.Services;

public class SimpleAuthStateProvider : AuthenticationStateProvider
{
    private const string StorageKey = "auth_user_id";

    private readonly IJSRuntime _js;
    private readonly AppDbContext _db;

    private static readonly ClaimsPrincipal Anonymous =
        new(new ClaimsIdentity());

    public SimpleAuthStateProvider(IJSRuntime js, AppDbContext db)
    {
        _js = js;
        _db = db;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var storedId = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);

        if (!int.TryParse(storedId, out var userId))
            return new AuthenticationState(Anonymous);

        var user = await _db.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            return new AuthenticationState(Anonymous);
        }

        // Map numeric RoleId to a readable role name
        var roleName = user.RoleId == 5 ? "Admin" : "Employee";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.Role, roleName)
        };

        //if (user.Employee != null)
        //{
            //claims.Add(new Claim("EmployeeId", user.Employee.EmployeeId.ToString()));
        //}

        var identity = new ClaimsIdentity(claims, "auth");
        var principal = new ClaimsPrincipal(identity);

        return new AuthenticationState(principal);
    }

    public async Task SignInAsync(int userId)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, userId.ToString());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }
}
