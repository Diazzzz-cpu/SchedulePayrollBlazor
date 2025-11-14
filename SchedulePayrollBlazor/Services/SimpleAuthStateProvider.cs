using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using SchedulePayrollBlazor.Data;

namespace SchedulePayrollBlazor.Services;

public class SimpleAuthStateProvider : AuthenticationStateProvider
{
    private const string StorageKey = "auth_user_id";

    private readonly IJSRuntime _js;
    private readonly IServiceScopeFactory _scopeFactory;

    private static readonly ClaimsPrincipal Anonymous =
        new(new ClaimsIdentity());

    public SimpleAuthStateProvider(IJSRuntime js, IServiceScopeFactory scopeFactory)
    {
        _js = js;
        _scopeFactory = scopeFactory;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var storedId = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);

        if (!int.TryParse(storedId, out var userId))
            return new AuthenticationState(Anonymous);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            return new AuthenticationState(Anonymous);
        }

        var roleName = user.RoleId == 5 ? "Admin" : "Employee";
        var displayName = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = user.Email;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.Role, roleName)
        };

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