namespace SchedulePayrollBlazor.Services.Models;

public sealed class WeekCopyResult
{
    public int TotalShiftsConsidered { get; set; }

    public int CreatedCount { get; set; }

    public int SkippedConflictsCount { get; set; }

    public static WeekCopyResult Empty => new();
}
