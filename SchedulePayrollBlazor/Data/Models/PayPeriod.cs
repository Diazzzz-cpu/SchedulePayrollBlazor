using System;
using System.Collections.Generic;

namespace SchedulePayrollBlazor.Data.Models;

public class PayPeriod
{
    public int Id { get; set; }

    public string PeriodName { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Status { get; set; } = "Open";

    public ICollection<PayrollRun> PayrollRuns { get; set; } = new List<PayrollRun>();
}
