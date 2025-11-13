using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages + Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// EF Core  use your existing connection string "Default"
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("Default");
    options.UseMySql(cs, ServerVersion.AutoDetect(cs));
});

// Simple auth
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, SimpleAuthStateProvider>();
builder.Services.AddScoped<SimpleAuthStateProvider>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");   // <- this must match the page name "_Host"

app.Run();
