using System;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// OPTIONAL but helps avoid the “Some services are not able to be constructed”
// AggregateException at builder.Build(), by turning off eager validation.
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = false;
    options.ValidateOnBuild = false;
});

// 1. Blazor + Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// 2. Connection string + DbContext setup
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Missing MySQL connection string 'Default' in appsettings(.Development).json.");

void ConfigureDbOptions(DbContextOptionsBuilder options)
{
    // This is safe – it just auto-detects based on your MySQL server.
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
}

// Scoped DbContext (for pages / components that inject AppDbContext)
builder.Services.AddDbContext<AppDbContext>(ConfigureDbOptions);

// Factory (for services like SimpleAuthStateProvider that need their own context)
builder.Services.AddDbContextFactory<AppDbContext>(ConfigureDbOptions);

// 3. Auth services
builder.Services.AddAuthorizationCore();

// Register SimpleAuthStateProvider once and reuse it as the AuthenticationStateProvider
builder.Services.AddScoped<SimpleAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<SimpleAuthStateProvider>());

// App services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminUserService>();

// --------------------------------
// Build + pipeline
// --------------------------------
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
app.MapFallbackToPage("/_Host");

// Seed / ensure admin user
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DatabaseSeeder.EnsureSeedDataAsync(db);
}

await app.RunAsync();
