using System;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Ensure appsettings.json + environment-specific settings are loaded
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

// OPTIONAL but helps avoid Some services are not able to be constructed
// AggregateException at builder.Build(), by turning off eager validation.
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = false;
    options.ValidateOnBuild = false;
});

// 1. Blazor + Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();

// 2. Connection string + DbContext setup
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Missing MySQL connection string 'Default' in appsettings.json (ConnectionStrings section).");

void ConfigureDbOptions(DbContextOptionsBuilder options)
{
    // Auto-detect server version based on your MySQL instance.
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
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IAttendanceSettingsService, AttendanceSettingsService>();

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

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

// --------------------------------
// Seed database (ensure admin, etc.)
// --------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DatabaseSeeder.EnsureSeedDataAsync(db);
}

// Run the app
await app.RunAsync();
