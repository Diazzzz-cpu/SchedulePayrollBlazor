using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Services.Models;

namespace SchedulePayrollBlazor.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AppDbContext _dbContext;
    private const int LateGraceMinutes = 5;
    private const int UndertimeGraceMinutes = 5;
    private const int OvertimeThresholdMinutes = 5;

    public AttendanceService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TimeLogResult> ClockInAsync(int employeeId)
    {
        var now = DateTime.Now;

        var openLog = await _dbContext.TimeLogs
            .Where(t => t.EmployeeId == employeeId && t.ClockOut == null)
            .OrderByDescending(t => t.ClockIn)
            .FirstOrDefaultAsync();

        if (openLog is not null)
        {
            return new TimeLogResult(false, "You are already clocked in. Please clock out first.", openLog);
        }

        var log = new TimeLog
        {
            EmployeeId = employeeId,
            ClockIn = now,
            Source = "Web"
        };

        _dbContext.TimeLogs.Add(log);
        await _dbContext.SaveChangesAsync();

        return new TimeLogResult(true, "Clocked in successfully.", log);
    }

    public async Task<TimeLogResult> ClockOutAsync(int employeeId)
    {
        var now = DateTime.Now;

        var openLog = await _dbContext.TimeLogs
            .Where(t => t.EmployeeId == employeeId && t.ClockOut == null)
            .OrderByDescending(t => t.ClockIn)
            .FirstOrDefaultAsync();

        if (openLog is null)
        {
            return new TimeLogResult(false, "You do not have an active time log to clock out from.", null);
        }

        openLog.ClockOut = now;
        await _dbContext.SaveChangesAsync();

        return new TimeLogResult(true, "Clocked out successfully.", openLog);
    }

    public async Task<List<DailyAttendanceDto>> GetAttendanceForEmployeeAsync(int employeeId, DateOnly start, DateOnly end)
    {
        var startDate = start.ToDateTime(TimeOnly.MinValue);
        var endDate = end.ToDateTime(TimeOnly.MaxValue);

        var logs = await _dbContext.TimeLogs
            .AsNoTracking()
            .Where(t => t.EmployeeId == employeeId && t.ClockIn >= startDate && t.ClockIn <= endDate)
            .OrderBy(t => t.ClockIn)
            .ToListAsync();

        var shifts = await _dbContext.Shifts
            .AsNoTracking()
            .Where(s => s.EmployeeId == employeeId && s.Start >= startDate && s.Start <= endDate)
            .OrderBy(s => s.Start)
            .ToListAsync();

        var logsByDate = logs
            .GroupBy(l => DateOnly.FromDateTime(l.ClockIn))
            .ToDictionary(g => g.Key, g => g.ToList());

        var shiftsByDate = shifts
            .GroupBy(s => DateOnly.FromDateTime(s.Start))
            .ToDictionary(g => g.Key, g => g.ToList());

        var dates = new HashSet<DateOnly>();
        foreach (var date in logsByDate.Keys)
        {
            dates.Add(date);
        }

        foreach (var date in shiftsByDate.Keys)
        {
            dates.Add(date);
        }

        return dates
            .Select(date =>
            {
                var logsForDate = logsByDate.TryGetValue(date, out var logList) ? logList : new List<TimeLog>();
                var shiftsForDate = shiftsByDate.TryGetValue(date, out var shiftList) ? shiftList : new List<Shift>();
                return BuildDailyAttendance(date, logsForDate, shiftsForDate);
            })
            .OrderByDescending(d => d.Date)
            .ToList();
    }

    public async Task<AttendancePeriodSummary> GetSummaryForEmployeeAsync(int employeeId, DateOnly start, DateOnly end)
    {
        var attendance = await GetAttendanceForEmployeeAsync(employeeId, start, end);

        var summary = new AttendancePeriodSummary
        {
            EmployeeId = employeeId,
            Days = attendance
        };

        foreach (var day in attendance)
        {
            summary.TotalRenderedTime += day.TotalDuration;
            summary.TotalLateMinutes += day.LateMinutes;
            summary.TotalUndertimeMinutes += day.UndertimeMinutes;
            summary.TotalOvertimeMinutes += day.OvertimeMinutes;

            if (day.ScheduledHours > 0)
            {
                summary.DaysWithShift++;
                if (day.IsAbsent)
                {
                    summary.FullDayAbsences++;
                }
            }
        }

        return summary;
    }

    public async Task<PaginatedAttendanceAdminView> GetAttendanceOverviewAsync(DateOnly date, int? employeeIdFilter, int page, int pageSize)
    {
        var dateStart = date.ToDateTime(TimeOnly.MinValue);
        var dateEnd = date.ToDateTime(TimeOnly.MaxValue);

        var scheduledEmployeeIdsQuery = _dbContext.Shifts
            .AsNoTracking()
            .Where(s => s.Start >= dateStart && s.Start <= dateEnd)
            .Select(s => s.EmployeeId)
            .Distinct();

        if (employeeIdFilter.HasValue)
        {
            scheduledEmployeeIdsQuery = scheduledEmployeeIdsQuery.Where(id => id == employeeIdFilter.Value);
        }

        var scheduledEmployeeIds = await scheduledEmployeeIdsQuery.ToListAsync();

        if (!scheduledEmployeeIds.Any())
        {
            return new PaginatedAttendanceAdminView
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = 0,
                Rows = new List<AttendanceAdminRow>()
            };
        }

        var employeesQuery = _dbContext.Employees
            .AsNoTracking()
            .Where(e => e.IsActive && scheduledEmployeeIds.Contains(e.EmployeeId))
            .OrderBy(e => e.FullName)
            .AsQueryable();

        var total = await employeesQuery.CountAsync();
        var employees = await employeesQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var employeeIds = employees.Select(e => e.EmployeeId).ToList();
        var logs = await _dbContext.TimeLogs
            .AsNoTracking()
            .Where(t => employeeIds.Contains(t.EmployeeId) && t.ClockIn >= dateStart && t.ClockIn <= dateEnd)
            .ToListAsync();

        var shifts = await _dbContext.Shifts
            .AsNoTracking()
            .Where(s => employeeIds.Contains(s.EmployeeId) && s.Start >= dateStart && s.Start <= dateEnd)
            .ToListAsync();

        var logsByEmployee = logs
            .GroupBy(l => (l.EmployeeId, DateOnly.FromDateTime(l.ClockIn)))
            .ToDictionary(g => g.Key, g => g.ToList());

        var shiftsByEmployee = shifts
            .GroupBy(s => (s.EmployeeId, DateOnly.FromDateTime(s.Start)))
            .ToDictionary(g => g.Key, g => g.ToList());

        var rows = employees.Select(e =>
        {
            var key = (e.EmployeeId, date);
            var employeeLogs = logsByEmployee.TryGetValue(key, out var logList) ? logList : new List<TimeLog>();
            var employeeShifts = shiftsByEmployee.TryGetValue(key, out var shiftList) ? shiftList : new List<Shift>();
            var attendance = BuildDailyAttendance(date, employeeLogs, employeeShifts);
            return new AttendanceAdminRow
            {
                EmployeeId = e.EmployeeId,
                EmployeeName = e.FullName,
                Attendance = attendance
            };
        }).ToList();

        return new PaginatedAttendanceAdminView
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Rows = rows
        };
    }

    private DailyAttendanceDto BuildDailyAttendance(DateOnly date, List<TimeLog> logs, List<Shift> shifts)
    {
        var hasLogs = logs.Any();

        var firstIn = logs.OrderBy(l => l.ClockIn).FirstOrDefault()?.ClockIn;
        var lastOut = logs
            .Where(l => l.ClockOut.HasValue)
            .OrderByDescending(l => l.ClockOut)
            .FirstOrDefault()?.ClockOut;

        var total = TimeSpan.Zero;
        foreach (var log in logs)
        {
            if (log.ClockOut.HasValue)
            {
                total += log.ClockOut.Value - log.ClockIn;
            }
        }

        var hasShift = shifts.Any();
        DateTime? shiftStart = null;
        DateTime? shiftEnd = null;
        decimal scheduledHours = 0m;
        var isLate = false;
        var isUndertime = false;
        var isOvertime = false;
        var isAbsent = false;
        var lateMinutes = 0;
        var undertimeMinutes = 0;
        var overtimeMinutes = 0;

        if (hasShift)
        {
            shiftStart = shifts.Min(s => s.Start);
            shiftEnd = shifts.Max(s => s.End);
            scheduledHours = (decimal)(shiftEnd.Value - shiftStart.Value).TotalHours;

            if (!hasLogs)
            {
                isAbsent = true;
            }
            else
            {
                if (firstIn.HasValue)
                {
                    var difference = (firstIn.Value - shiftStart.Value).TotalMinutes - LateGraceMinutes;
                    lateMinutes = (int)Math.Max(0, Math.Round(difference, MidpointRounding.AwayFromZero));
                    isLate = lateMinutes > 0;
                }

                if (lastOut.HasValue)
                {
                    var undertimeDifference = (shiftEnd.Value - lastOut.Value).TotalMinutes - UndertimeGraceMinutes;
                    undertimeMinutes = (int)Math.Max(0, Math.Round(undertimeDifference, MidpointRounding.AwayFromZero));
                    isUndertime = undertimeMinutes > 0;

                    var overtimeDifference = (lastOut.Value - shiftEnd.Value).TotalMinutes - OvertimeThresholdMinutes;
                    overtimeMinutes = (int)Math.Max(0, Math.Round(overtimeDifference, MidpointRounding.AwayFromZero));
                    isOvertime = overtimeMinutes > 0;
                }
            }
        }

        return new DailyAttendanceDto
        {
            Date = date,
            FirstIn = firstIn,
            LastOut = lastOut,
            TotalDuration = hasLogs ? total : TimeSpan.Zero,
            ScheduledStart = shiftStart,
            ScheduledEnd = shiftEnd,
            Logs = logs,
            HasLogs = hasLogs,
            IsLate = isLate,
            IsUndertime = isUndertime,
            IsOvertime = isOvertime,
            IsAbsent = isAbsent,
            LateMinutes = lateMinutes,
            UndertimeMinutes = undertimeMinutes,
            OvertimeMinutes = overtimeMinutes,
            ScheduledHours = scheduledHours
        };
    }
}
