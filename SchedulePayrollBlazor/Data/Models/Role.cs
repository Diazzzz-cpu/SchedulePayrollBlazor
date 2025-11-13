using System;
using System.Collections.Generic;

namespace SchedulePayrollBlazor.Data.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
