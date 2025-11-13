using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SchedulePayrollBlazor.Data;

namespace SchedulePayrollBlazor.Services;

public class SimpleAuthStateProvider : AuthenticationStateProvider
{
    private const string StorageKey = "auth_user_id";

    private readonly IJSRuntime _jsRuntime;
    private readonly AppDbContext _dbContext;
    private static readonly ClaimsPrincipal AnonymousPrincipal = new(new ClaimsIdentity());

    private AuthenticationState _cachedState = new AuthenticationState(AnonymousPrincipal);
    private AuthUserSnapshot _snapshot = AuthUserSnapshot.Anonymous();
    private bool _isLoaded;

    public SimpleAuthStateProvider(IJSRuntime jsRuntime, AppDbContext dbContext)
    {
        _jsRuntime = jsRuntime;
        _dbContext = dbContext;
    }

    public bool IsAuthenticated => _snapshot.IsAuthenticated;

    public bool IsAdmin => _snapshot.IsAdmin;

    public string? CurrentRole => _snapshot.Role;

    public int? CurrentUserId => _snapshot.UserId;

    public int? CurrentEmployeeId => _snapshot.EmployeeId;

    public string? CurrentEmail => _snapshot.Email;

    public string? CurrentDisplayName => _snapshot.DisplayName;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => LoadAuthenticationStateAsync();

    public async Task<AuthUserSnapshot> GetCurrentUserAsync()
    {
        if (!_isLoaded)
        {
            await LoadAuthenticationStateAsync();
        }

        return _snapshot;
    }

    public async Task SignInAsync(int userId)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, userId.ToString(CultureInfo.InvariantCulture));
        NotifyAuthenticationStateChanged(LoadAuthenticationStateAsync(forceReload: true));
    }

    public async Task SignOutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        _isLoaded = false;
        _snapshot = AuthUserSnapshot.Anonymous();
        _cachedState = new AuthenticationState(AnonymousPrincipal);
        NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
    }

    private async Task<AuthenticationState> LoadAuthenticationStateAsync(bool forceReload = false)
    {
        if (!forceReload && _isLoaded)
        {
            return _cachedState;
        }

        var userIdValue = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (!int.TryParse(userIdValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var userId))
        {
            _snapshot = AuthUserSnapshot.Anonymous();
            _cachedState = new AuthenticationState(AnonymousPrincipal);
            _isLoaded = true;
            return _cachedState;
        }

        var user = await _dbContext.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            _snapshot = AuthUserSnapshot.Anonymous();
            _cachedState = new AuthenticationState(AnonymousPrincipal);
            _isLoaded = true;
            return _cachedState;
        }

        var employeeId = user.Employee?.Id;
        var displayName = user.Employee?.FullName;
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = user.Email;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, (employeeId ?? user.Id).ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("UserId", user.Id.ToString(CultureInfo.InvariantCulture))
        };

        if (employeeId.HasValue)
        {
            claims.Add(new Claim("EmployeeId", employeeId.Value.ToString(CultureInfo.InvariantCulture)));
        }

        _snapshot = new AuthUserSnapshot(true, user.Id, employeeId, user.Role, user.Email, displayName);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "SimpleAuth"));
        _cachedState = new AuthenticationState(principal);
        _isLoaded = true;
        return _cachedState;
    }

    public readonly record struct AuthUserSnapshot(
        bool IsAuthenticated,
        int? UserId,
        int? EmployeeId,
        string? Role,
        string? Email,
        string? DisplayName)
    {
        public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);

        public static AuthUserSnapshot Anonymous() => new(false, null, null, null, null, null);
    }
}
