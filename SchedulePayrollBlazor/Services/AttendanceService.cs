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
    private const int GraceMinutes = 10;

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

        return logs
            .GroupBy(l => DateOnly.FromDateTime(l.ClockIn))
            .Select(g => BuildDailyAttendance(g.Key, g.ToList(), null))
            .OrderByDescending(d => d.Date)
            .ToList();
    }

    public async Task<PaginatedAttendanceAdminView> GetAttendanceOverviewAsync(DateOnly date, int? employeeIdFilter, int page, int pageSize)
    {
        var query = _dbContext.Employees
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.FullName)
            .AsQueryable();

        if (employeeIdFilter.HasValue)
        {
            query = query.Where(e => e.EmployeeId == employeeIdFilter.Value);
        }

        var total = await query.CountAsync();
        var employees = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var dateStart = date.ToDateTime(TimeOnly.MinValue);
        var dateEnd = date.ToDateTime(TimeOnly.MaxValue);

        var employeeIds = employees.Select(e => e.EmployeeId).ToList();
        var logs = await _dbContext.TimeLogs
            .AsNoTracking()
            .Where(t => employeeIds.Contains(t.EmployeeId) && t.ClockIn >= dateStart && t.ClockIn <= dateEnd)
            .ToListAsync();

        var rows = employees.Select(e =>
        {
            var employeeLogs = logs.Where(l => l.EmployeeId == e.EmployeeId).ToList();
            var attendance = BuildDailyAttendance(date, employeeLogs, null);
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

    private DailyAttendanceDto BuildDailyAttendance(DateOnly date, List<TimeLog> logs, Shift? shift)
    {
        var firstIn = logs.OrderBy(l => l.ClockIn).FirstOrDefault()?.ClockIn;
        var lastOut = logs.OrderByDescending(l => l.ClockOut ?? l.ClockIn).FirstOrDefault()?.ClockOut;

        var total = TimeSpan.Zero;
        foreach (var log in logs)
        {
            if (log.ClockOut.HasValue)
            {
                total += log.ClockOut.Value - log.ClockIn;
            }
        }

        bool isLate = false;
        bool isUndertime = false;

        if (shift is not null && firstIn.HasValue)
        {
            isLate = firstIn.Value > shift.Start.AddMinutes(GraceMinutes);
        }

        if (shift is not null && lastOut.HasValue)
        {
            isUndertime = lastOut.Value < shift.End.AddMinutes(-GraceMinutes);
        }

        return new DailyAttendanceDto
        {
            Date = date,
            FirstIn = firstIn,
            LastOut = lastOut,
            TotalDuration = total,
            Logs = logs,
            IsLate = isLate,
            IsUndertime = isUndertime
        };
    }
}
