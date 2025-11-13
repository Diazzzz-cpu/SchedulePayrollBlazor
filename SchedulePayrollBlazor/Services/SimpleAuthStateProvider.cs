using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Services;

public class SimpleAuthStateProvider : AuthenticationStateProvider
{   
    private readonly IJSRuntime _js;
    private readonly AppDbContext _db;
    private static readonly ClaimsPrincipal _anon = new(new ClaimsIdentity());

    public SimpleAuthStateProvider(IJSRuntime js, AppDbContext db)
    {
        _js = js;
        _db = db;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var email = await _js.InvokeAsync<string>("localStorage.getItem", "auth_email");
        if (string.IsNullOrWhiteSpace(email))
            return new AuthenticationState(_anon);

        var emp = await _db.Set<Employee>()
                           .Include(e => e.Role)
                           .FirstOrDefaultAsync(e => e.Email == email);

        if (emp is null || emp.Active != true)
            return new AuthenticationState(_anon);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, emp.Name ?? email),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.NameIdentifier, emp.EmployeeId.ToString()),
            new Claim(ClaimTypes.Role, emp.Role?.Name ?? "User"),
            new Claim("RoleCode", emp.Role?.Code ?? "USER")
        };

        var identity = new ClaimsIdentity(claims, "Simple");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task SignIn(string email)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", "auth_email", email);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOut()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "auth_email");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
