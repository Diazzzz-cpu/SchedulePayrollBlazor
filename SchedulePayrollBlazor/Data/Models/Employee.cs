using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SchedulePayrollBlazor.Data.Models;

[Table("employee")]
public class Employee
{
    [Key]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("department")]
    public string Department { get; set; } = string.Empty;

    [Column("job_title")]
    public string JobTitle { get; set; } = string.Empty;

    [Column("employment_type")]
    public string EmploymentType { get; set; } = "FullTime";

    [Column("start_date")]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [Column("location")]
    public string Location { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public ICollection<PayrollRun> PayrollRuns { get; set; } = new List<PayrollRun>();

    public string FullName
    {
        get
        {
            var parts = new[] { FirstName, LastName }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            if (parts.Length == 0 && User is not null)
            {
                parts = new[] { User.FirstName, User.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            }

            return string.Join(" ", parts);
        }
    }
}
