using System;
using System.Collections.Generic;

namespace SchedulePayrollBlazor.Data.Models;

public partial class Payrollline
{
    public int PayrollLineId { get; set; }

    public int PayrollRunId { get; set; }

    public int PayComponentId { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? Rate { get; set; }

    public decimal Amount { get; set; }

    public virtual Paycomponent PayComponent { get; set; } = null!;

    public virtual Payrollrun PayrollRun { get; set; } = null!;
}
