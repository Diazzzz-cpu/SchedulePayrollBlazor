using SchedulePayrollBlazor.Services.Models;

namespace SchedulePayrollBlazor.Services;

public interface IEmployeeComponentService
{
    Task<BulkAssignResult> BulkAssignAsync(
        IReadOnlyCollection<int> employeeIds,
        IReadOnlyCollection<BulkAssignComponentRequest> components,
        CancellationToken cancellationToken = default);
}
