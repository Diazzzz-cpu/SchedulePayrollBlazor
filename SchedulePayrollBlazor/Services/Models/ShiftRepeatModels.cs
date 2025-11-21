using System;
using System.Collections.Generic;
using System.Linq;

namespace SchedulePayrollBlazor.Services.Models;

public enum ShiftRepeatMode
{
    None = 0,
    Weekly,
    Weekdays,
    CustomDays
}

public sealed class ShiftRepeatRequest
{
    public ShiftRepeatMode RepeatMode { get; set; }

    public DateOnly? RepeatUntil { get; set; }

    public IReadOnlyCollection<DayOfWeek> RepeatDays { get; set; } = Array.Empty<DayOfWeek>();
}

public sealed class ShiftOperationResult
{
    public int Total { get; set; }

    public int Created { get; set; }

    public int SkippedConflicts { get; set; }

    public static ShiftOperationResult Empty => new();

    public static ShiftOperationResult Combine(params ShiftOperationResult[] results)
    {
        if (results is null || results.Length == 0)
        {
            return Empty;
        }

        return new ShiftOperationResult
        {
            Total = results.Sum(r => r.Total),
            Created = results.Sum(r => r.Created),
            SkippedConflicts = results.Sum(r => r.SkippedConflicts)
        };
    }
}
