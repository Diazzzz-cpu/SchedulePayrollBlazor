using System;
using System.Collections.Generic;

namespace SchedulePayrollBlazor.Data.Models;

public partial class Timelog
{
    public int TimeLogId { get; set; }

    public int EmployeeId { get; set; }

    public DateTime ClockIn { get; set; }

    public DateTime ClockOut { get; set; }

    public string? Source { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}
