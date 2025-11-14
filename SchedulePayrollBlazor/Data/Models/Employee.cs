using System;
using System.Collections.Generic;
using System.Linq;

namespace SchedulePayrollBlazor.Data.Models;

public class Employee
{
    public int EmployeeId { get; set; }

    public int UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string EmploymentType { get; set; } = "FullTime";

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public string Location { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public ICollection<PayrollRun> PayrollRuns { get; set; } = new List<PayrollRun>();

    public string FullName => string.Join(" ", new[] { FirstName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)));
}
