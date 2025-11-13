using System;
using System.Collections.Generic;

namespace SchedulePayrollBlazor.Data.Models;

public partial class Payrollrun
{
    public int PayrollRunId { get; set; }

    public int PayPeriodId { get; set; }

    public int EmployeeId { get; set; }

    public string? Status { get; set; }

    public decimal? GrossPay { get; set; }

    public decimal? TotalDeductions { get; set; }

    public decimal? NetPay { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Payperiod PayPeriod { get; set; } = null!;

    public virtual ICollection<Payrollline> Payrolllines { get; set; } = new List<Payrollline>();
}
