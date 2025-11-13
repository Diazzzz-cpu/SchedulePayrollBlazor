using System;
using System.Collections.Generic;

namespace SchedulePayrollBlazor.Data.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public int RoleId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string EmploymentClass { get; set; } = null!;

    public string EmploymentType { get; set; } = null!;

    public decimal HourlyRate { get; set; }

    public decimal MonthlyRate { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<Employeecomponent> Employeecomponents { get; set; } = new List<Employeecomponent>();

    public virtual ICollection<Payrollrun> Payrollruns { get; set; } = new List<Payrollrun>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<Timelog> Timelogs { get; set; } = new List<Timelog>();
}
