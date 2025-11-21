using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Services.Models;

namespace SchedulePayrollBlazor.Services;

public interface IRolePayComponentService
{
    Task<List<RolePayComponent>> GetForRoleAsync(int roleId);
    Task SetDefaultsForRoleAsync(int roleId, IEnumerable<RolePayComponentDefinition> definitions);
    Task<EmployeeComponentApplyResult> ApplyDefaultsToEmployeeAsync(int employeeId);
}
