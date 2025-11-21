using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Services.Models;

namespace SchedulePayrollBlazor.Services;

public class RolePayComponentService : IRolePayComponentService
{
    private readonly AppDbContext _dbContext;

    public RolePayComponentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<RolePayComponent>> GetForRoleAsync(int roleId)
    {
        return _dbContext.RolePayComponents
            .Where(rp => rp.RoleId == roleId && rp.IsActive)
            .Include(rp => rp.PayComponent)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task SetDefaultsForRoleAsync(int roleId, IEnumerable<RolePayComponentDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var existing = await _dbContext.RolePayComponents
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();

        var definitionLookup = definitions
            .GroupBy(d => d.PayComponentId)
            .ToDictionary(g => g.Key, g => g.Last());

        foreach (var item in definitionLookup.Values)
        {
            var existingEntry = existing.FirstOrDefault(rp => rp.PayComponentId == item.PayComponentId);
            if (existingEntry is null)
            {
                _dbContext.RolePayComponents.Add(new RolePayComponent
                {
                    RoleId = roleId,
                    PayComponentId = item.PayComponentId,
                    DefaultRateOverride = item.DefaultRateOverride,
                    IsActive = item.IsActive
                });
            }
            else
            {
                existingEntry.DefaultRateOverride = item.DefaultRateOverride;
                existingEntry.IsActive = item.IsActive;
            }
        }

        foreach (var stale in existing)
        {
            if (!definitionLookup.ContainsKey(stale.PayComponentId))
            {
                stale.IsActive = false;
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<EmployeeComponentApplyResult> ApplyDefaultsToEmployeeAsync(int employeeId)
    {
        var employee = await _dbContext.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId)
            ?? throw new InvalidOperationException("Unable to find employee.");

        var roleComponents = await _dbContext.RolePayComponents
            .Where(rp => rp.RoleId == employee.User!.RoleId && rp.IsActive)
            .ToListAsync();

        var payComponentIds = roleComponents.Select(rp => rp.PayComponentId).ToList();
        var payComponents = await _dbContext.PayComponents
            .Where(pc => payComponentIds.Contains(pc.PayComponentId))
            .ToDictionaryAsync(pc => pc.PayComponentId);

        var existingAssignments = await _dbContext.EmployeeComponents
            .Where(ec => ec.EmployeeId == employeeId && ec.Active)
            .Select(ec => ec.PayComponentId)
            .ToListAsync();

        var result = new EmployeeComponentApplyResult();

        foreach (var rp in roleComponents)
        {
            if (existingAssignments.Contains(rp.PayComponentId))
            {
                result.SkippedCount++;
                continue;
            }

            payComponents.TryGetValue(rp.PayComponentId, out var payComponent);

            _dbContext.EmployeeComponents.Add(new EmployeeComponent
            {
                EmployeeId = employeeId,
                PayComponentId = rp.PayComponentId,
                Amount = rp.DefaultRateOverride ?? payComponent?.DefaultAmount ?? 0m,
                Active = true
            });

            result.CreatedCount++;
        }

        await _dbContext.SaveChangesAsync();
        return result;
    }
}
