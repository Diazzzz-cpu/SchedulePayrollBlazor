using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Services.Models;

namespace SchedulePayrollBlazor.Services;

public class EmployeeComponentService : IEmployeeComponentService
{
    private readonly AppDbContext _dbContext;

    public EmployeeComponentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BulkAssignResult> BulkAssignAsync(
        IReadOnlyCollection<int> employeeIds,
        IReadOnlyCollection<BulkAssignComponentRequest> components,
        CancellationToken cancellationToken = default)
    {
        if (employeeIds.Count == 0 || components.Count == 0)
        {
            return new BulkAssignResult();
        }

        var payComponentIds = components.Select(c => c.PayComponentId).ToList();

        var existingAssignments = await _dbContext.EmployeeComponents
            .Where(ec => employeeIds.Contains(ec.EmployeeId) && payComponentIds.Contains(ec.PayComponentId) && ec.Active)
            .ToListAsync(cancellationToken);

        var existingLookup = existingAssignments
            .GroupBy(ec => (ec.EmployeeId, ec.PayComponentId))
            .ToDictionary(g => g.Key, g => g.First());

        var payComponents = await _dbContext.PayComponents
            .Where(pc => payComponentIds.Contains(pc.PayComponentId))
            .ToDictionaryAsync(pc => pc.PayComponentId, cancellationToken);

        var result = new BulkAssignResult
        {
            EmployeesConsidered = employeeIds.Count
        };

        foreach (var employeeId in employeeIds)
        {
            foreach (var component in components)
            {
                var key = (employeeId, component.PayComponentId);
                if (existingLookup.ContainsKey(key))
                {
                    result.AssignmentsSkippedExisting++;
                    continue;
                }

                payComponents.TryGetValue(component.PayComponentId, out var payComponent);

                _dbContext.EmployeeComponents.Add(new EmployeeComponent
                {
                    EmployeeId = employeeId,
                    PayComponentId = component.PayComponentId,
                    Amount = component.CustomRate ?? payComponent?.DefaultAmount ?? 0m,
                    Active = true
                });

                result.AssignmentsCreated++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
