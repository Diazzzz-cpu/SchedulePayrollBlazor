using System;
using System.Collections.Generic;

namespace SchedulePayrollBlazor.Data.Models;

public partial class Employeecomponent
{
    public int EmployeeComponentId { get; set; }

    public int EmployeeId { get; set; }

    public int PayComponentId { get; set; }

    public decimal Amount { get; set; }

    public bool? Active { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Paycomponent PayComponent { get; set; } = null!;
}
