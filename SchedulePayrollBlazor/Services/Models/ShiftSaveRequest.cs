using SchedulePayrollBlazor.Data.Models;

namespace SchedulePayrollBlazor.Services.Models;

public sealed class ShiftSaveRequest
{
    public Shift Shift { get; init; } = default!;

    public ShiftRepeatRequest RepeatRequest { get; init; } = new();
}
