using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SchedulePayrollBlazor.Data;

namespace SchedulePayrollBlazor.Services;

public class SimpleAuthStateProvider : AuthenticationStateProvider
{
    private const string StorageKey = "auth_user_id";

    private readonly IJSRuntime _jsRuntime;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    private static readonly ClaimsPrincipal AnonymousPrincipal = new(new ClaimsIdentity());

    public SimpleAuthStateProvider(IJSRuntime jsRuntime, IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _jsRuntime = jsRuntime;
        _dbContextFactory = dbContextFactory;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Read userId from browser localStorage
        var storedId = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (!int.TryParse(storedId, out var userId))
        {
            return new AuthenticationState(AnonymousPrincipal);
        }

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var user = await dbContext.Users
            .Include(u => u.Role)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user is null)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            return new AuthenticationState(AnonymousPrincipal);
        }

        // Role model now uses Name instead of RoleName
        var roleName = NormalizeRoleName(user.Role?.Name);

        // Build display name:
        // 1) Employee.FullName if present
        // 2) User.FirstName + User.LastName
        // 3) Fallback to email
        string displayName =
            user.Employee?.FullName ??
            $"{user.FirstName} {user.LastName}".Trim();

        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = user.Email;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.Role, roleName)
        };

        if (user.Employee is not null)
        {
            claims.Add(new Claim("EmployeeId", user.Employee.EmployeeId.ToString()));
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "server-auth");
        var principal = new ClaimsPrincipal(identity);

        return new AuthenticationState(principal);
    }

    public async Task SignInAsync(int userId)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, userId.ToString());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(AnonymousPrincipal)));
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

        return roleName;
    }
}
