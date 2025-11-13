using System;
using System.Collections.Generic;

namespace SchedulePayrollBlazor.Data.Models;

public partial class Paycomponent
{
    public int PayComponentId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Kind { get; set; } = null!;

    public decimal? DefaultRate { get; set; }

    public virtual ICollection<Employeecomponent> Employeecomponents { get; set; } = new List<Employeecomponent>();

    public virtual ICollection<Payrollline> Payrolllines { get; set; } = new List<Payrollline>();
}
