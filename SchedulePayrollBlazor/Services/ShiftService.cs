using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;
using SchedulePayrollBlazor.Services.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace SchedulePayrollBlazor.Services;

public class ShiftService : IShiftService
{
    private readonly AppDbContext _db;

    private const string OverlapErrorMessage = "Schedule overlaps with the current set of schedules for this employee.";

    public ShiftService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Shift>> GetShiftsAsync(
        DateTime weekStart,
        DateTime weekEnd,
        int? employeeId = null,
        string? group = null,
        string? searchText = null)
    {
        try
        {
            // Normalize to date-only boundaries so we don't get bitten by time/Kind differences.
            var startDate = weekStart.Date;
            var endDate = weekEnd.Date;

            IQueryable<Shift> query = _db.Shifts
                .AsNoTracking()
                .Where(s => s.Start.Date >= startDate && s.Start.Date < endDate);

            if (employeeId.HasValue)
            {
                query = query.Where(s => s.EmployeeId == employeeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(group) &&
                !string.Equals(group, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(s => s.GroupName != null && s.GroupName == group);
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var term = searchText.Trim().ToLowerInvariant();
                query = query.Where(s =>
                    s.EmployeeName != null &&
                    s.EmployeeName.ToLower().Contains(term));
            }

            return await query
                .OrderBy(s => s.Start)
                .ToListAsync();
        }
        catch
        {
            return Array.Empty<Shift>();
        }
    }

    public Task<Shift?> GetShiftByIdAsync(int id)
    {
        return _db.Shifts
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<int?> ResolveEmployeeIdForUserAsync(string? userIdClaim, string? email)
    {
        if (int.TryParse(userIdClaim, out var userId) && userId > 0)
        {
            var employeeId = await _db.Users
                .AsNoTracking()
                .Where(u => u.UserId == userId)
                .Select(u => u.Employee != null ? (int?)u.Employee.EmployeeId : null)
                .FirstOrDefaultAsync();

            if (employeeId.HasValue)
            {
                return employeeId.Value;
            }
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            var normalizedEmail = email.Trim();
            var employeeId = await _db.Users
                .AsNoTracking()
                .Where(u => u.Email == normalizedEmail)
                .Select(u => u.Employee != null ? (int?)u.Employee.EmployeeId : null)
                .FirstOrDefaultAsync();

            if (employeeId.HasValue)
            {
                return employeeId.Value;
            }
        }

        return null;
    }

    public async Task<IReadOnlyList<Shift>> GetShiftsForEmployeeAsync(int employeeId, DateTime startInclusive, DateTime endExclusive)
    {
        if (employeeId <= 0)
        {
            return Array.Empty<Shift>();
        }

        try
        {
            var startDate = startInclusive.Date;
            var endDate = endExclusive.Date;

            return await _db.Shifts
                .AsNoTracking()
                .Where(s =>
                    s.EmployeeId == employeeId &&
                    s.Start.Date >= startDate &&
                    s.Start.Date < endDate)
                .OrderBy(s => s.Start)
                .ToListAsync();
        }
        catch
        {
            return Array.Empty<Shift>();
        }
    }

    public async Task<List<Shift>> GetShiftHistoryForEmployeeAsync(int employeeId, DateTime? from = null, DateTime? to = null)
    {
        if (employeeId <= 0)
        {
            return new List<Shift>();
        }

        // Use local "now" instead of UTC since we treat all times as PH local
        var upperBound = to ?? DateTime.Now;
        var lowerBound = from ?? upperBound.AddMonths(-3);

        try
        {
            return await _db.Shifts
                .AsNoTracking()
                .Where(s => s.EmployeeId == employeeId)
                .Where(s => s.End <= upperBound)
                .Where(s => s.Start >= lowerBound)
                .OrderByDescending(s => s.Start)
                .ToListAsync();
        }
        catch
        {
            return new List<Shift>();
        }
    }

    public async Task<Shift?> GetShiftForEmployeeOnAsync(int employeeId, DateTime day)
    {
        var start = day.Date;
        var end = start.AddDays(1);

        return await _db.Shifts
            .AsNoTracking()
            .Where(s => s.EmployeeId == employeeId && s.Start >= start && s.Start < end)
            .OrderBy(s => s.Start)
            .FirstOrDefaultAsync();
    }

    public async Task<Shift> InsertShiftAsync(Shift shift)
    {
        ArgumentNullException.ThrowIfNull(shift);

        if (shift.Start >= shift.End)
        {
            throw new ArgumentException("Shift start must be before the end time.", nameof(shift));
        }

        shift.EmployeeName = string.IsNullOrWhiteSpace(shift.EmployeeName)
            ? shift.EmployeeName
            : shift.EmployeeName.Trim();

        if (await HasOverlappingShiftAsync(shift.EmployeeId, shift.Start, shift.End))
        {
            throw new InvalidOperationException(OverlapErrorMessage);
        }

        await ApplyEmployeeMetadataAsync(
            shift,
            preferExistingName: !string.IsNullOrWhiteSpace(shift.EmployeeName),
            preferExistingGroup: !string.IsNullOrWhiteSpace(shift.GroupName));

        await _db.Shifts.AddAsync(shift);
        await _db.SaveChangesAsync();

        return shift;
    }

    public async Task<Shift> UpdateShiftAsync(Shift shift)
    {
        ArgumentNullException.ThrowIfNull(shift);

        if (shift.Start >= shift.End)
        {
            throw new ArgumentException("Shift start must be before the end time.", nameof(shift));
        }

        var existing = await _db.Shifts.FirstOrDefaultAsync(s => s.Id == shift.Id)
            ?? throw new KeyNotFoundException($"Shift with id {shift.Id} was not found.");

        if (await HasOverlappingShiftAsync(shift.EmployeeId, shift.Start, shift.End, shift.Id))
        {
            throw new InvalidOperationException(OverlapErrorMessage);
        }

        existing.EmployeeId = shift.EmployeeId;
        existing.Start = shift.Start;
        existing.End = shift.End;
        var hasCustomName = !string.IsNullOrWhiteSpace(shift.EmployeeName);
        if (hasCustomName)
        {
            existing.EmployeeName = shift.EmployeeName!.Trim();
        }

        var hasCustomGroup = !string.IsNullOrWhiteSpace(shift.GroupName);
        existing.GroupName = NormalizeGroup(shift.GroupName);

        await ApplyEmployeeMetadataAsync(
            existing,
            preferExistingName: hasCustomName,
            preferExistingGroup: hasCustomGroup);

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteShiftAsync(int id)
    {
        var entity = await _db.Shifts.FirstOrDefaultAsync(s => s.Id == id);
        if (entity is null)
        {
            return false;
        }

        _db.Shifts.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public Task<bool> HasOverlappingShiftAsync(int employeeId, DateTime start, DateTime end, int? shiftIdToExclude = null)
    {
        if (employeeId <= 0)
        {
            return Task.FromResult(false);
        }

        return _db.Shifts
            .AsNoTracking()
            .Where(s => s.EmployeeId == employeeId)
            .Where(s => !shiftIdToExclude.HasValue || s.Id != shiftIdToExclude.Value)
            .AnyAsync(s => start < s.End && end > s.Start);
    }

    public async Task<WeekCopyResult> CopyWeekToNextAsync(DateOnly sourceWeekStart, CancellationToken cancellationToken = default)
    {
        var sourceStart = sourceWeekStart.ToDateTime(TimeOnly.MinValue);
        var sourceEnd = sourceStart.AddDays(7);
        const int WeekOffsetDays = 7;

        var sourceShifts = await _db.Shifts
            .AsNoTracking()
            .Where(s => s.Start >= sourceStart && s.Start < sourceEnd)
            .ToListAsync(cancellationToken);

        var result = new WeekCopyResult
        {
            TotalShiftsConsidered = sourceShifts.Count,
            CreatedCount = 0,
            SkippedConflictsCount = 0
        };

        if (sourceShifts.Count == 0)
        {
            return result;
        }

        var newShifts = new List<Shift>();

        foreach (var shift in sourceShifts)
        {
            var newStart = shift.Start.AddDays(WeekOffsetDays);
            var newEnd = shift.End.AddDays(WeekOffsetDays);

            if (await HasConflictAsync(shift.EmployeeId, newStart, newEnd, null, cancellationToken))
            {
                result.SkippedConflictsCount++;
                continue;
            }

            var clone = CloneShift(shift, newStart, newEnd);
            await ApplyEmployeeMetadataAsync(clone, preferExistingName: true, preferExistingGroup: true);
            newShifts.Add(clone);
            result.CreatedCount++;
        }

        if (newShifts.Count > 0)
        {
            await _db.Shifts.AddRangeAsync(newShifts, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private Task<bool> HasConflictAsync(int employeeId, DateTime start, DateTime end, int? excludeShiftId, CancellationToken cancellationToken)
    {
        return _db.Shifts
            .AsNoTracking()
            .AnyAsync(s =>
                s.EmployeeId == employeeId &&
                (!excludeShiftId.HasValue || s.Id != excludeShiftId.Value) &&
                s.Start < end &&
                s.End > start,
                cancellationToken);
    }

    public async Task<WeekCopyResult> CopyWeekAsync(
        DateOnly sourceWeekStart,
        int numberOfWeeksToCopy,
        CancellationToken cancellationToken = default)
    {
        if (numberOfWeeksToCopy <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfWeeksToCopy));
        }

        var sourceStart = sourceWeekStart.ToDateTime(TimeOnly.MinValue);
        var sourceEnd = sourceStart.AddDays(7);

        List<Shift> sourceShifts;
        try
        {
            sourceShifts = await _db.Shifts
                .AsNoTracking()
                .Where(s => s.Start >= sourceStart && s.Start < sourceEnd)
                .OrderBy(s => s.Start)
                .ToListAsync(cancellationToken);
        }
        catch
        {
            return WeekCopyResult.Empty;
        }

        var result = new WeekCopyResult
        {
            TotalShiftsConsidered = sourceShifts.Count * numberOfWeeksToCopy
        };

        if (sourceShifts.Count == 0)
        {
            return result;
        }

        var newShifts = new List<Shift>();

        for (var offset = 1; offset <= numberOfWeeksToCopy; offset++)
        {
            var offsetDays = 7 * offset;

            foreach (var shift in sourceShifts)
            {
                var targetStart = shift.Start.AddDays(offsetDays);
                var targetEnd = shift.End.AddDays(offsetDays);

                if (await HasOverlappingShiftAsync(shift.EmployeeId, targetStart, targetEnd))
                {
                    result.SkippedConflictsCount++;
                    continue;
                }

                var clone = CloneShift(shift, targetStart, targetEnd);
                await ApplyEmployeeMetadataAsync(clone, preferExistingName: true, preferExistingGroup: true);
                newShifts.Add(clone);
                result.CreatedCount++;
            }
        }

        if (newShifts.Count > 0)
        {
            await _db.Shifts.AddRangeAsync(newShifts, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public async Task<ShiftOperationResult> CreateRepeatedShiftsAsync(Shift baseShift, ShiftRepeatRequest repeatRequest)
    {
        ArgumentNullException.ThrowIfNull(baseShift);
        ArgumentNullException.ThrowIfNull(repeatRequest);

        if (repeatRequest.RepeatMode == ShiftRepeatMode.None || !repeatRequest.RepeatUntil.HasValue)
        {
            return ShiftOperationResult.Empty;
        }

        var untilDate = repeatRequest.RepeatUntil.Value;
        var baseDate = DateOnly.FromDateTime(baseShift.Start.Date);
        if (untilDate < baseDate)
        {
            return ShiftOperationResult.Empty;
        }

        var candidates = GenerateRepeatDates(baseDate, untilDate, repeatRequest);
        var duration = baseShift.End - baseShift.Start;
        var result = new ShiftOperationResult();
        var newShifts = new List<Shift>();

        foreach (var date in candidates)
        {
            var targetStart = date.ToDateTime(TimeOnly.FromTimeSpan(baseShift.Start.TimeOfDay));
            var targetEnd = targetStart.Add(duration);

            result.Total++;

            if (await HasOverlappingShiftAsync(baseShift.EmployeeId, targetStart, targetEnd))
            {
                result.SkippedConflicts++;
                continue;
            }

            var clone = new Shift
            {
                EmployeeId = baseShift.EmployeeId,
                EmployeeName = baseShift.EmployeeName,
                GroupName = baseShift.GroupName,
                Start = targetStart,
                End = targetEnd
            };

            await ApplyEmployeeMetadataAsync(clone, preferExistingName: true, preferExistingGroup: true);
            newShifts.Add(clone);
            result.Created++;
        }

        if (newShifts.Count > 0)
        {
            await _db.Shifts.AddRangeAsync(newShifts);
            await _db.SaveChangesAsync();
        }

        return result;
    }

    private async Task ApplyEmployeeMetadataAsync(Shift shift, bool preferExistingName = false, bool preferExistingGroup = false)
    {
        var normalizedGroup = NormalizeGroup(shift.GroupName);
        shift.GroupName = normalizedGroup;

        var employee = await _db.Employees
            .AsNoTracking()
            .Select(e => new { e.EmployeeId, e.FullName, e.Department })
            .FirstOrDefaultAsync(e => e.EmployeeId == shift.EmployeeId);

        if (employee is not null)
        {
            if (!preferExistingName)
            {
                var resolvedName = string.IsNullOrWhiteSpace(employee.FullName)
                    ? $"Employee {employee.EmployeeId}"
                    : employee.FullName!;
                shift.EmployeeName = resolvedName;
            }

            if (!preferExistingGroup)
            {
                shift.GroupName = NormalizeGroup(string.IsNullOrWhiteSpace(employee.Department)
                    ? null
                    : employee.Department);
            }
        }
        else
        {
            if (!preferExistingName)
            {
                shift.EmployeeName = string.IsNullOrWhiteSpace(shift.EmployeeName)
                    ? $"Employee {shift.EmployeeId}"
                    : shift.EmployeeName.Trim();
            }
        }
    }

    private static string? NormalizeGroup(string? group)
    {
        if (string.IsNullOrWhiteSpace(group))
        {
            return null;
        }

        return group.Trim();
    }

    private static Shift CloneShift(Shift source, DateTime targetStart, DateTime targetEnd)
    {
        return new Shift
        {
            EmployeeId = source.EmployeeId,
            EmployeeName = source.EmployeeName,
            GroupName = source.GroupName,
            Start = targetStart,
            End = targetEnd
        };
    }

    private static IEnumerable<DateOnly> GenerateRepeatDates(DateOnly baseDate, DateOnly until, ShiftRepeatRequest request)
    {
        var start = baseDate.AddDays(1);

        switch (request.RepeatMode)
        {
            case ShiftRepeatMode.Weekly:
                for (var date = baseDate.AddDays(7); date <= until; date = date.AddDays(7))
                {
                    yield return date;
                }

                yield break;

            case ShiftRepeatMode.Weekdays:
                for (var date = start; date <= until; date = date.AddDays(1))
                {
                    if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    {
                        continue;
                    }

                    yield return date;
                }

                yield break;

            case ShiftRepeatMode.CustomDays:
                var daySet = new HashSet<DayOfWeek>(request.RepeatDays ?? Array.Empty<DayOfWeek>());

                for (var date = start; date <= until; date = date.AddDays(1))
                {
                    if (daySet.Contains(date.DayOfWeek))
                    {
                        yield return date;
                    }
                }

                yield break;
            default:
                yield break;
        }
    }
}
