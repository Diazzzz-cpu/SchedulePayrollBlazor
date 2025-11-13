using System;

namespace SchedulePayrollBlazor.Data.Models;

public class Schedule
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateOnly ShiftDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Source { get; set; } = "Manual";

    public Employee Employee { get; set; } = null!;
}
