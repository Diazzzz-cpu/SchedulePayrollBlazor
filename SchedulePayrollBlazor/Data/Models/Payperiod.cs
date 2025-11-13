using System;
using System.Collections.Generic;

namespace SchedulePayrollBlazor.Data.Models;

public partial class Payperiod
{
    public int PayPeriodId { get; set; }

    public string PeriodName { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Payrollrun> Payrollruns { get; set; } = new List<Payrollrun>();
}
