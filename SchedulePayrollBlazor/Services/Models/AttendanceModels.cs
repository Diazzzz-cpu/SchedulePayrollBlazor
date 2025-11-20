using System;
using System.Collections.Generic;

namespace SchedulePayrollBlazor.Services.Models;

public record TimeLogResult(bool Success, string Message, Data.Models.TimeLog? Log);

public class DailyAttendanceDto
{
    public DateOnly Date { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public DateTime? FirstIn { get; set; }
    public DateTime? LastOut { get; set; }
    public bool IsLate { get; set; }
    public bool IsUndertime { get; set; }
    public bool HasLogs => Logs.Count > 0;
    public List<Data.Models.TimeLog> Logs { get; set; } = new();
}

public class AttendanceAdminRow
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DailyAttendanceDto Attendance { get; set; } = new();
}

public class PaginatedAttendanceAdminView
{
    public List<AttendanceAdminRow> Rows { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
