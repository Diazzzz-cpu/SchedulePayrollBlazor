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
        var distinctEmployees = employeeIds.Distinct().ToList();
        var distinctComponents = components
            .GroupBy(c => c.PayComponentId)
            .Select(g => g.Last())
            .ToList();

        if (distinctEmployees.Count == 0 || distinctComponents.Count == 0)
        {
            return new BulkAssignResult();
        }

        var payComponentIds = distinctComponents.Select(c => c.PayComponentId).ToList();

        var existingAssignments = await _dbContext.EmployeeComponents
            .Where(ec => distinctEmployees.Contains(ec.EmployeeId)
                         && payComponentIds.Contains(ec.PayComponentId)
                         && ec.Active)
            .ToListAsync(cancellationToken);

        var existingLookup = existingAssignments
            .GroupBy(ec => (ec.EmployeeId, ec.PayComponentId))
            .ToDictionary(g => g.Key, g => g.First());

        var payComponents = await _dbContext.PayComponents
            .Where(pc => payComponentIds.Contains(pc.PayComponentId))
            .ToDictionaryAsync(pc => pc.PayComponentId, cancellationToken);

        var result = new BulkAssignResult
        {
            EmployeesConsidered = distinctEmployees.Count
        };

        foreach (var employeeId in distinctEmployees)
        {
            foreach (var component in distinctComponents)
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

                existingLookup[key] = new EmployeeComponent
                {
                    EmployeeId = employeeId,
                    PayComponentId = component.PayComponentId
                };
                result.AssignmentsCreated++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
