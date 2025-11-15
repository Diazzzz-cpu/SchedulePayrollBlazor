using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulePayrollBlazor.Data;
using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Services;

public interface IShiftService
{
    Task<IReadOnlyList<Shift>> GetShiftsAsync(
        DateTime weekStart,
        DateTime weekEnd,
        int? employeeId = null,
        string? group = null,
        string? searchText = null);
}

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
        IQueryable<Shift> query = _db.Shifts
            .AsNoTracking()
            .Where(s => s.Start >= weekStart && s.Start < weekEnd);

        if (employeeId.HasValue)
        {
            query = query.Where(s => s.EmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(group) && !string.Equals(group, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(s => s.GroupName == group);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim().ToLowerInvariant();
            query = query.Where(s => s.EmployeeName != null && s.EmployeeName.ToLower().Contains(term));
        }

        return await query
            .OrderBy(s => s.Start)
            .ToListAsync();
    }
}
