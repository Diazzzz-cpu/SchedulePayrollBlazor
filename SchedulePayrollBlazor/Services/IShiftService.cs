using SchedulePayrollBlazor.Data.Models;
using System;
using System.Collections.Generic;

namespace SchedulePayrollBlazor.Services;

public interface IShiftService
{
    Task<IReadOnlyList<Shift>> GetShiftsAsync(
        DateTime weekStart,
        DateTime weekEnd,
        int? employeeId = null,
        string? group = null,
        string? searchText = null);

    Task<Shift?> GetShiftByIdAsync(int id);

    Task<int?> ResolveEmployeeIdForUserAsync(string? userIdClaim, string? email);

    Task<IReadOnlyList<Shift>> GetShiftsForEmployeeAsync(int employeeId, DateTime startInclusive, DateTime endExclusive);

    Task<List<Shift>> GetShiftHistoryForEmployeeAsync(int employeeId, DateTime? from = null, DateTime? to = null);

    Task<Shift> InsertShiftAsync(Shift shift);

    /// <summary>
    /// Updates an existing shift. Throws <see cref="KeyNotFoundException"/> if the shift cannot be found.
    /// </summary>
    Task<Shift> UpdateShiftAsync(Shift shift);

    Task<bool> DeleteShiftAsync(int id);
}
