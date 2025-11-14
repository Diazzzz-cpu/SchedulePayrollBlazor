using System;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Missing MySQL connection string 'Default'.");

void ConfigureDbOptions(DbContextOptionsBuilder options)
    => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

builder.Services.AddDbContext<AppDbContext>(ConfigureDbOptions);
builder.Services.AddDbContextFactory<AppDbContext>(ConfigureDbOptions);

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<SimpleAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<SimpleAuthStateProvider>());

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminUserService>();

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DatabaseSeeder.EnsureSeedDataAsync(db);
}

await app.RunAsync();
