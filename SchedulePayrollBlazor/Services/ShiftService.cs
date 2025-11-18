using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;
using System;
using System.Linq;

namespace SchedulePayrollBlazor.Services;

public class ShiftService : IShiftService
{
    private readonly AppDbContext _db;

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
}
