using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchedulePayrollBlazor.Services.Models;

namespace SchedulePayrollBlazor.Services;

public interface IAttendanceService
{
    Task<TimeLogResult> ClockInAsync(int employeeId);
    Task<TimeLogResult> ClockOutAsync(int employeeId);
    Task<List<DailyAttendanceDto>> GetAttendanceForEmployeeAsync(int employeeId, DateOnly start, DateOnly end);
    Task<PaginatedAttendanceAdminView> GetAttendanceOverviewAsync(DateOnly date, int? employeeIdFilter, int page, int pageSize);
}
