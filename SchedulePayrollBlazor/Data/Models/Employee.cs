using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchedulePayrollBlazor.Data.Models;

public enum EmploymentType
{
    FullTime,
    PartTime,
    Contractor,
    Intern
}

[Table("employees")]
public class Employee
{
    [Key]
    [Column("employee_id")]
    public int EmployeeId { get; set; }

    // Link to users table
    [Column("user_id")]
    public int UserId { get; set; }

    // Main display name for dashboards/lists
    [Required]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Column("department")]
    public string? Department { get; set; }
        = string.Empty;

    [Column("job_title")]
    public string? JobTitle { get; set; }
        = string.Empty;

    [Column("employment_type")]
    public string EmploymentTypeValue { get; set; }
        = EmploymentType.FullTime.ToString();

    [NotMapped]
    public EmploymentType EmploymentType
    {
        get => Enum.TryParse<EmploymentType>(EmploymentTypeValue, true, out var parsed)
            ? parsed
            : EmploymentType.FullTime;
        set => EmploymentTypeValue = value.ToString();
    }

    // Nullable so we can safely use HasValue / ??
    [Column("start_date")]
    public DateTime? StartDate { get; set; }
        = null;

    // Used by AdminUserService, Profile, AdminDashboard
    [Column("location")]
    public string? Location { get; set; }
        = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [NotMapped]
    public string PasswordHash
    {
        get => User?.PasswordHash ?? string.Empty;
        set
        {
            if (User is not null)
            {
                User.PasswordHash = value;
            }
        }
    }

    // Navigation to User
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
        = default!;

    // Navigation to EmployeeProfile (used in AppDbContext)
    public EmployeeProfile? Profile { get; set; }
        = default!;

    public ICollection<EmployeeComponent> EmployeeComponents { get; set; }
        = new List<EmployeeComponent>();

    public EmployeeCompensation? Compensation { get; set; }
        = default;

    public ICollection<PayrollRun> PayrollRuns { get; set; }
        = new List<PayrollRun>();

    public ICollection<PayrollEntry> PayrollEntries { get; set; }
        = new List<PayrollEntry>();

    public ICollection<Schedule> Schedules { get; set; }
        = new List<Schedule>();

    public ICollection<TimeLog> TimeLogs { get; set; }
        = new List<TimeLog>();
}
